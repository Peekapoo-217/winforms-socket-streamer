using System.Net;
using System.Net.Sockets;

namespace NetworkCore.Events;

/// <summary>
/// Event data raised when a UDP datagram is received by <see cref="AsyncUdpMessenger"/>.
/// </summary>
public sealed class UdpDataReceivedEventArgs : EventArgs
{
    /// <summary>The remote endpoint that sent the datagram.</summary>
    public IPEndPoint RemoteEndPoint { get; }

    /// <summary>The raw payload bytes received.</summary>
    public byte[] Data { get; }

    public UdpDataReceivedEventArgs(IPEndPoint remoteEndPoint, byte[] data)
    {
        RemoteEndPoint = remoteEndPoint;
        Data = data;
    }
}
