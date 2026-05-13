namespace NetworkCore.Protocol;

/// <summary>
/// Identifies the type of payload carried in a packet.
/// Stored as the first byte of every message frame.
/// </summary>
public enum DataType : byte
{
    /// <summary>UTF-8 encoded text message.</summary>
    Text = 1,

    /// <summary>JPEG-compressed screen capture frame.</summary>
    Image = 2,

    /// <summary>Serialized mouse input command (position + button state).</summary>
    MouseCommand = 3,

    /// <summary>Serialized keyboard input command.</summary>
    KeyboardCommand = 4,

    /// <summary>Control/handshake messages (e.g. resolution info, ping).</summary>
    Control = 5,

    /// <summary>Client → Server: carries the PIN string for authentication.</summary>
    Authentication = 6,

    /// <summary>Server → Client: carries the authentication result (success/failure).</summary>
    AuthResponse = 7,

    // ─── Relay Server Protocol ───
    RegisterHost = 8,
    HostRegistered = 9,
    JoinSession = 10,
    SessionPaired = 11,
    SessionError = 12,

    /// <summary>Viewer → Host: dynamic quality/interval adjustment command.</summary>
    QualityCommand = 13
}
