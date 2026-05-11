namespace NetworkCore.Events;

/// <summary>
/// Event data raised when a remote client disconnects from the server.
/// </summary>
public sealed class ClientDisconnectedEventArgs : EventArgs
{
    /// <summary>
    /// The unique identifier of the disconnected client session.
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// Optional reason or exception message describing why the client disconnected.
    /// Null when the client disconnected gracefully.
    /// </summary>
    public string? Reason { get; }

    public ClientDisconnectedEventArgs(string clientId, string? reason = null)
    {
        ClientId = clientId;
        Reason = reason;
    }
}
