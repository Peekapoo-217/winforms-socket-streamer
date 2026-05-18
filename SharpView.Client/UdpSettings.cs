namespace SharpView.Client;

/// <summary>
/// Configurable UDP settings for the typing indicator feature.
/// Centralizes the UDP port to avoid hardcoding in logic.
/// </summary>
internal static class UdpSettings
{
    /// <summary>
    /// Local UDP port used by the Client (Viewer) for typing indicator signals.
    /// Must differ from the Server's port when running on the same machine.
    /// </summary>
    internal const int DefaultUdpPort = 10001;
}
