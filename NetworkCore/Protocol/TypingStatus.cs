namespace NetworkCore.Protocol;

/// <summary>
/// Single-byte flag transmitted via UDP to indicate typing state.
/// </summary>
public enum TypingStatus : byte
{
    /// <summary>User has stopped typing (or idle timeout).</summary>
    Stopped = 0x00,

    /// <summary>User is currently typing.</summary>
    Typing = 0x01
}
