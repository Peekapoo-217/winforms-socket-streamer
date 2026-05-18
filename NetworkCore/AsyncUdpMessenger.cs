using System.Net;
using System.Net.Sockets;
using NetworkCore.Events;

namespace NetworkCore;

/// <summary>
/// Asynchronous UDP messenger for lightweight, fire-and-forget signals
/// (e.g. typing indicators). Wraps <see cref="UdpClient"/> with
/// async send/receive and an event-driven receive loop.
/// </summary>
public sealed class AsyncUdpMessenger : IDisposable
{
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cts;
    private bool _disposed;
    private readonly int _listenPort;

    /// <summary>Raised on the background thread when a UDP datagram arrives.</summary>
    public event EventHandler<UdpDataReceivedEventArgs>? DataReceived;

    /// <summary>The local port this messenger is listening on.</summary>
    public int ListenPort => _listenPort;

    /// <param name="listenPort">Local UDP port to bind for receiving.</param>
    public AsyncUdpMessenger(int listenPort)
    {
        if (listenPort is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(listenPort));
        _listenPort = listenPort;
    }

    // ───────────────────────── Public API ─────────────────────────

    /// <summary>
    /// Binds to the configured port and starts the async receive loop.
    /// </summary>
    public void StartListening()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _udpClient = new UdpClient(_listenPort);
        _cts = new CancellationTokenSource();
        _ = ReceiveLoopAsync(_cts.Token);
    }

    /// <summary>
    /// Sends a UDP datagram asynchronously to the specified target.
    /// </summary>
    public async Task SendSignalAsync(IPAddress targetIp, int targetPort, byte[] payload)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_udpClient is null)
            throw new InvalidOperationException("Messenger has not started listening.");

        await _udpClient.SendAsync(payload, payload.Length,
            new IPEndPoint(targetIp, targetPort)).ConfigureAwait(false);
    }

    // ───────────────────────── Private Logic ─────────────────────────

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _udpClient is not null)
            {
                var result = await _udpClient.ReceiveAsync(ct).ConfigureAwait(false);
                DataReceived?.Invoke(this,
                    new UdpDataReceivedEventArgs(result.RemoteEndPoint, result.Buffer));
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        catch (SocketException) { /* Port closed or unreachable */ }
    }

    // ───────────────────────── Helpers ─────────────────────────

    /// <summary>
    /// Resolves the best local LAN IPv4 address by probing a dummy connection.
    /// </summary>
    public static IPAddress GetLocalLanIp()
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect("8.8.8.8", 80); // no data is actually sent
            return ((IPEndPoint)socket.LocalEndPoint!).Address;
        }
        catch
        {
            return IPAddress.Loopback;
        }
    }

    // ───────────────────────── IDisposable ─────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        _udpClient?.Close();
        _udpClient?.Dispose();
    }
}
