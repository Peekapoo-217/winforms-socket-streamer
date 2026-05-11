namespace NetworkCore.Events;

/// <summary>
/// Event data raised when a complete data frame is received from a remote peer.
/// </summary>
public sealed class DataReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The identifier of the sender.
    /// For server-side events this is the client ID; for client-side events this is "Server".
    /// </summary>
    public string SenderId { get; }

    /// <summary>
    /// The raw payload bytes received (after length-prefix removal).
    /// </summary>
    public byte[] Data { get; }

    public DataReceivedEventArgs(string senderId, byte[] data)
    {
        SenderId = senderId;
        Data = data;
    }
}
