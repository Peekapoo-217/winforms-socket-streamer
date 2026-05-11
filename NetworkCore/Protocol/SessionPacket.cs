using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetworkCore.Protocol;

/// <summary>
/// Payload for relay server session management.
/// Carries Partner ID + Password for the TeamViewer-style authentication model.
/// </summary>
public sealed class SessionPacket
{
    /// <summary>9-digit Partner ID derived from hardware UUID (persistent per machine).</summary>
    [JsonPropertyName("partnerId")]
    public string PartnerId { get; set; } = string.Empty;

    /// <summary>4-digit random password generated each time the Host starts.</summary>
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Human-readable message (e.g. error reason, success confirmation).</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public byte[] ToBytes() => JsonSerializer.SerializeToUtf8Bytes(this, s_jsonOptions);

    public static SessionPacket FromBytes(byte[] data) =>
        JsonSerializer.Deserialize<SessionPacket>(data, s_jsonOptions)
        ?? throw new InvalidDataException("Failed to deserialize SessionPacket.");

    /// <summary>Generic builder: creates a tagged packet from a fully populated SessionPacket.</summary>
    public static byte[] BuildPacket(DataType type, SessionPacket packet)
    {
        return PacketBuilder.Build(type, packet.ToBytes());
    }

    // ─── Convenience factories ───

    /// <summary>Host → Relay: register with Partner ID and Password.</summary>
    public static byte[] BuildRegisterHost(string partnerId, string password)
    {
        var p = new SessionPacket { PartnerId = partnerId, Password = password };
        return PacketBuilder.Build(DataType.RegisterHost, p.ToBytes());
    }

    /// <summary>Relay → Host: confirm registration with the Partner ID.</summary>
    public static byte[] BuildHostRegistered(string partnerId, string message = "")
    {
        var p = new SessionPacket { PartnerId = partnerId, Message = message };
        return PacketBuilder.Build(DataType.HostRegistered, p.ToBytes());
    }

    /// <summary>Viewer → Relay: join using target Partner ID + Password.</summary>
    public static byte[] BuildJoinSession(string partnerId, string password)
    {
        var p = new SessionPacket { PartnerId = partnerId, Password = password };
        return PacketBuilder.Build(DataType.JoinSession, p.ToBytes());
    }

    /// <summary>Relay → both: session has been paired.</summary>
    public static byte[] BuildSessionPaired(string partnerId, string message = "")
    {
        var p = new SessionPacket { PartnerId = partnerId, Message = message };
        return PacketBuilder.Build(DataType.SessionPaired, p.ToBytes());
    }

    /// <summary>Relay → Viewer: error (wrong ID / wrong password).</summary>
    public static byte[] BuildSessionError(string partnerId, string message)
    {
        var p = new SessionPacket { PartnerId = partnerId, Message = message };
        return PacketBuilder.Build(DataType.SessionError, p.ToBytes());
    }
}
