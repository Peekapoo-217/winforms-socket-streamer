namespace SharpView.Server;

/// <summary>
/// Configurable UDP settings for the typing indicator feature.
/// Centralizes the UDP port to avoid hardcoding in logic.
/// </summary>
internal static class UdpSettings
{
    /// <summary>
    /// Local UDP port used by the Server (Host) for typing indicator signals.
    /// Must differ from the Client's port when running on the same machine.
    /// </summary>
    internal const int DefaultUdpPort = 10002;
}
