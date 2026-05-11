using System.Net;

namespace NetworkCore.Events;

/// <summary>
/// Event data raised when a remote client connects to the server.
/// </summary>
public sealed class ClientConnectedEventArgs : EventArgs
{
    /// <summary>
    /// A unique identifier assigned to this client session.
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// The remote endpoint (IP:Port) of the connected client.
    /// </summary>
    public IPEndPoint RemoteEndPoint { get; }

    public ClientConnectedEventArgs(string clientId, IPEndPoint remoteEndPoint)
    {
        ClientId = clientId;
        RemoteEndPoint = remoteEndPoint;
    }
}
