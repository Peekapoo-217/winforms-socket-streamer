using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using NetworkCore.Events;

namespace NetworkCore;

/// <summary>
/// A fully asynchronous TCP server that accepts multiple client connections,
/// reads length-prefixed data frames, and exposes events for connection lifecycle
/// and data reception.
///
/// <para><b>Packet Framing Protocol:</b> Every message is preceded by a 4-byte
/// big-endian length header so the receiver can reconstruct complete frames
/// regardless of TCP segment boundaries.</para>
/// </summary>
public sealed class AsyncTcpServer : IDisposable
{
    // ───────────────────────── Fields ─────────────────────────

    private readonly IPAddress _ipAddress;
    private readonly int _port;
    private readonly int _bufferSize;

    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    /// <summary>
    /// Thread-safe dictionary mapping client IDs to their <see cref="TcpClient"/> instances.
    /// </summary>
    private readonly ConcurrentDictionary<string, TcpClient> _clients = new();

    /// <summary>
    /// Per-client write locks to prevent concurrent frame interleaving.
    /// </summary>
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _clientSendLocks = new();

    // ───────────────────────── Events ─────────────────────────

    /// <summary>Raised when a new client connects.</summary>
    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;

    /// <summary>Raised when a client disconnects (gracefully or due to error).</summary>
    public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

    /// <summary>Raised when a complete data frame has been received from a client.</summary>
    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    /// <summary>Raised when an internal error occurs.</summary>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    // ───────────────────────── Constructor ─────────────────────────

    /// <summary>
    /// Initializes a new <see cref="AsyncTcpServer"/>.
    /// </summary>
    /// <param name="ipAddress">The IP address to bind the listener to.</param>
    /// <param name="port">The TCP port to listen on.</param>
    /// <param name="bufferSize">
    /// Size of the read buffer in bytes. Defaults to 8 KB.
    /// Increase this for high-throughput scenarios (e.g. screen streaming).
    /// </param>
    public AsyncTcpServer(IPAddress ipAddress, int port, int bufferSize = 8192)
    {
        _ipAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));

        if (port is < 0 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 0 and 65535.");

        if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be positive.");

        _port = port;
        _bufferSize = bufferSize;
    }

    // ───────────────────────── Public API ─────────────────────────

    /// <summary>
    /// Starts listening for incoming TCP connections.
    /// Returns immediately; client-accept loop runs on a background task.
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _cts = new CancellationTokenSource();
        _listener = new TcpListener(_ipAddress, _port);
        _listener.Start();

        // Fire-and-forget the accept loop; errors are surfaced via events.
        _ = AcceptClientsAsync(_cts.Token);
    }

    /// <summary>
    /// Sends a length-prefixed data frame to a specific connected client.
    /// </summary>
    /// <param name="clientId">Target client identifier.</param>
    /// <param name="data">Raw payload bytes to send.</param>
    public async Task SendDataAsync(string clientId, byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_clients.TryGetValue(clientId, out var client))
            throw new InvalidOperationException($"Client '{clientId}' is not connected.");

        var sendLock = _clientSendLocks.GetOrAdd(clientId, _ => new SemaphoreSlim(1, 1));
        await sendLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await SendFrameAsync(client.GetStream(), data).ConfigureAwait(false);
        }
        finally
        {
            sendLock.Release();
        }
    }

    /// <summary>
    /// Broadcasts a length-prefixed data frame to all connected clients.
    /// </summary>
    /// <param name="data">Raw payload bytes to broadcast.</param>
    public async Task BroadcastDataAsync(byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var tasks = new List<Task>();

        foreach (var kvp in _clients)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await SendFrameAsync(kvp.Value.GetStream(), data).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    OnErrorOccurred($"Broadcast to {kvp.Key}", ex);
                }
            }));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Disconnects a specific client and removes it from the tracked collection.
    /// </summary>
    public void DisconnectClient(string clientId)
    {
        if (_clients.TryRemove(clientId, out var client))
        {
            client.Close();
            client.Dispose();
            OnClientDisconnected(clientId, "Disconnected by server.");
        }
    }

    /// <summary>
    /// Returns the number of currently connected clients.
    /// </summary>
    public int ConnectedClientsCount => _clients.Count;

    /// <summary>
    /// Gracefully stops the server: cancels the accept loop, disconnects all
    /// clients, and releases the listener.
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();

        foreach (var kvp in _clients)
        {
            kvp.Value.Close();
            kvp.Value.Dispose();
        }

        _clients.Clear();
    }

    // ───────────────────────── Private Logic ─────────────────────────

    /// <summary>
    /// Continuously accepts incoming TCP connections until cancellation is requested.
    /// Each accepted client is handled on its own background task.
    /// </summary>
    private async Task AcceptClientsAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var tcpClient = await _listener!.AcceptTcpClientAsync(ct).ConfigureAwait(false);
                var clientId = Guid.NewGuid().ToString("N")[..8]; // short unique id

                if (_clients.TryAdd(clientId, tcpClient))
                {
                    _clientSendLocks.TryAdd(clientId, new SemaphoreSlim(1, 1));
                    var remoteEp = (IPEndPoint)tcpClient.Client.RemoteEndPoint!;
                    OnClientConnected(clientId, remoteEp);

                    // Handle this client on a dedicated task.
                    _ = HandleClientAsync(clientId, tcpClient, ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown – swallow.
        }
        catch (Exception ex)
        {
            OnErrorOccurred("AcceptClients", ex);
        }
    }

    /// <summary>
    /// Reads length-prefixed frames from a single client until disconnection.
    /// </summary>
    private async Task HandleClientAsync(string clientId, TcpClient tcpClient, CancellationToken ct)
    {
        try
        {
            var stream = tcpClient.GetStream();
            var buffer = new byte[_bufferSize];

            while (!ct.IsCancellationRequested && tcpClient.Connected)
            {
                // 1. Read the 4-byte length header.
                var lengthBytes = await ReadExactAsync(stream, 4, ct).ConfigureAwait(false);
                if (lengthBytes is null) break; // graceful disconnect

                var frameLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBytes, 0));

                if (frameLength <= 0 || frameLength > 10_000_000) // 10 MB safety cap
                {
                    OnErrorOccurred($"Client {clientId}", new InvalidDataException($"Invalid frame length: {frameLength}"));
                    break;
                }

                // 2. Read the full payload.
                var payload = await ReadExactAsync(stream, frameLength, ct).ConfigureAwait(false);
                if (payload is null) break;

                OnDataReceived(clientId, payload);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown.
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"HandleClient({clientId})", ex);
        }
        finally
        {
            if (_clients.TryRemove(clientId, out _))
            {
                tcpClient.Close();
                tcpClient.Dispose();
                if (_clientSendLocks.TryRemove(clientId, out var sl)) sl.Dispose();
                OnClientDisconnected(clientId);
            }
        }
    }

    /// <summary>
    /// Reads exactly <paramref name="count"/> bytes from <paramref name="stream"/>.
    /// Returns <c>null</c> when the remote side closes the connection.
    /// </summary>
    private static async Task<byte[]?> ReadExactAsync(NetworkStream stream, int count, CancellationToken ct)
    {
        var buffer = new byte[count];
        var offset = 0;

        while (offset < count)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(offset, count - offset), ct).ConfigureAwait(false);

            if (bytesRead == 0)
                return null; // connection closed

            offset += bytesRead;
        }

        return buffer;
    }

    /// <summary>
    /// Writes a 4-byte big-endian length header + payload as a SINGLE contiguous
    /// buffer to eliminate the risk of interleaved writes.
    /// </summary>
    private static async Task SendFrameAsync(NetworkStream stream, byte[] data)
    {
        var header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
        var frame = new byte[4 + data.Length];
        Buffer.BlockCopy(header, 0, frame, 0, 4);
        Buffer.BlockCopy(data, 0, frame, 4, data.Length);
        await stream.WriteAsync(frame).ConfigureAwait(false);
        await stream.FlushAsync().ConfigureAwait(false);
    }

    // ───────────────────────── Event Raisers ─────────────────────────

    private void OnClientConnected(string clientId, IPEndPoint remoteEndPoint)
        => ClientConnected?.Invoke(this, new ClientConnectedEventArgs(clientId, remoteEndPoint));

    private void OnClientDisconnected(string clientId, string? reason = null)
        => ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(clientId, reason));

    private void OnDataReceived(string clientId, byte[] data)
        => DataReceived?.Invoke(this, new DataReceivedEventArgs(clientId, data));

    private void OnErrorOccurred(string context, Exception ex)
        => ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(context, ex));

    // ───────────────────────── IDisposable ─────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Stop();
        _cts?.Dispose();
    }
}
