using System.Net;
using System.Net.Sockets;
using NetworkCore.Events;

namespace NetworkCore;

/// <summary>
/// A fully asynchronous TCP client that connects to a remote server,
/// reads length-prefixed data frames, and exposes events for connection lifecycle
/// and data reception.
///
/// <para><b>Packet Framing Protocol:</b> Every message is preceded by a 4-byte
/// big-endian length header, matching the protocol used by <see cref="AsyncTcpServer"/>.</para>
/// </summary>
public sealed class AsyncTcpClient : IDisposable
{
    // ───────────────────────── Fields ─────────────────────────

    private readonly string _serverIp;
    private readonly int _serverPort;
    private readonly int _bufferSize;

    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    /// <summary>
    /// Serializes concurrent writes to the NetworkStream.
    /// Without this, two async tasks (e.g. StreamLoop + MonitorLoop) can
    /// interleave their header/payload bytes, corrupting the framing protocol.
    /// </summary>
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    // ───────────────────────── Events ─────────────────────────

    /// <summary>Raised when the client successfully connects to the server.</summary>
    public event EventHandler<EventArgs>? Connected;

    /// <summary>Raised when the client disconnects from the server.</summary>
    public event EventHandler<ClientDisconnectedEventArgs>? Disconnected;

    /// <summary>Raised when a complete data frame has been received from the server.</summary>
    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    /// <summary>Raised when an internal error occurs.</summary>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    // ───────────────────────── Properties ─────────────────────────

    /// <summary>
    /// Indicates whether the underlying TCP connection is currently active.
    /// </summary>
    public bool IsConnected => _tcpClient?.Connected ?? false;

    // ───────────────────────── Constructor ─────────────────────────

    /// <summary>
    /// Initializes a new <see cref="AsyncTcpClient"/>.
    /// </summary>
    /// <param name="serverIp">The IP address or hostname of the server to connect to.</param>
    /// <param name="serverPort">The TCP port of the server.</param>
    /// <param name="bufferSize">
    /// Size of the read buffer in bytes. Defaults to 8 KB.
    /// </param>
    public AsyncTcpClient(string serverIp, int serverPort, int bufferSize = 8192)
    {
        if (string.IsNullOrWhiteSpace(serverIp))
            throw new ArgumentException("Server IP cannot be null or empty.", nameof(serverIp));

        if (serverPort is < 0 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(serverPort), "Port must be between 0 and 65535.");

        if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be positive.");

        _serverIp = serverIp;
        _serverPort = serverPort;
        _bufferSize = bufferSize;
    }

    // ───────────────────────── Public API ─────────────────────────

    /// <summary>
    /// Asynchronously connects to the remote server and begins the receive loop.
    /// </summary>
    public async Task ConnectAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _cts = new CancellationTokenSource();
        _tcpClient = new TcpClient();

        await _tcpClient.ConnectAsync(_serverIp, _serverPort).ConfigureAwait(false);
        _stream = _tcpClient.GetStream();

        OnConnected();

        // Start the background receive loop.
        _ = ReceiveLoopAsync(_cts.Token);
    }

    /// <summary>
    /// Sends a length-prefixed data frame to the connected server.
    /// </summary>
    /// <param name="data">Raw payload bytes to send.</param>
    public async Task SendDataAsync(byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_stream is null || !IsConnected)
            throw new InvalidOperationException("Client is not connected to any server.");

        await _sendLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await SendFrameAsync(_stream, data).ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    /// <summary>
    /// Gracefully disconnects from the server and releases resources.
    /// </summary>
    public void Disconnect()
    {
        _cts?.Cancel();

        _stream?.Close();
        _stream?.Dispose();
        _stream = null;

        _tcpClient?.Close();
        _tcpClient?.Dispose();
        _tcpClient = null;

        OnDisconnected("Disconnected by local call.");
    }

    // ───────────────────────── Private Logic ─────────────────────────

    /// <summary>
    /// Continuously reads length-prefixed frames from the server until
    /// cancellation or disconnection.
    /// </summary>
    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _stream is not null)
            {
                // 1. Read the 4-byte length header.
                var lengthBytes = await ReadExactAsync(_stream, 4, ct).ConfigureAwait(false);
                if (lengthBytes is null) break; // server closed connection

                var frameLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBytes, 0));

                if (frameLength <= 0 || frameLength > 10_000_000) // 10 MB safety cap
                {
                    OnErrorOccurred("ReceiveLoop", new InvalidDataException($"Invalid frame length: {frameLength}"));
                    break;
                }

                // 2. Read the full payload.
                var payload = await ReadExactAsync(_stream, frameLength, ct).ConfigureAwait(false);
                if (payload is null) break;

                OnDataReceived(payload);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during disconnect.
        }
        catch (Exception ex)
        {
            OnErrorOccurred("ReceiveLoop", ex);
        }
        finally
        {
            OnDisconnected("Connection lost.");
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
    /// buffer to eliminate the risk of interleaved writes from concurrent tasks.
    /// </summary>
    private static async Task SendFrameAsync(NetworkStream stream, byte[] data)
    {
        var header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));

        // Combine header + payload into one buffer so it goes out in a single WriteAsync call.
        var frame = new byte[4 + data.Length];
        Buffer.BlockCopy(header, 0, frame, 0, 4);
        Buffer.BlockCopy(data, 0, frame, 4, data.Length);

        await stream.WriteAsync(frame).ConfigureAwait(false);
        await stream.FlushAsync().ConfigureAwait(false);
    }

    // ───────────────────────── Event Raisers ─────────────────────────

    private void OnConnected()
        => Connected?.Invoke(this, EventArgs.Empty);

    private void OnDisconnected(string? reason)
        => Disconnected?.Invoke(this, new ClientDisconnectedEventArgs("Self", reason));

    private void OnDataReceived(byte[] data)
        => DataReceived?.Invoke(this, new DataReceivedEventArgs("Server", data));

    private void OnErrorOccurred(string context, Exception ex)
        => ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(context, ex));

    // ───────────────────────── IDisposable ─────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Disconnect();
        _cts?.Dispose();
        _sendLock.Dispose();
    }
}
