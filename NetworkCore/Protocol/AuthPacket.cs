using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetworkCore.Protocol;

/// <summary>
/// Payload for the PIN-based authentication handshake.
///
/// <para><b>Client → Server:</b> Carries the PIN entered by the user.
/// Sent as <see cref="DataType.Authentication"/>.</para>
///
/// <para><b>Server → Client:</b> Carries the result.
/// Sent as <see cref="DataType.AuthResponse"/>.</para>
/// </summary>
public sealed class AuthPacket
{
    /// <summary>The 4-digit PIN string (client → server).</summary>
    [JsonPropertyName("pin")]
    public string Pin { get; set; } = string.Empty;

    /// <summary>
    /// <c>true</c> = authentication succeeded; <c>false</c> = rejected.
    /// Only meaningful in <see cref="DataType.AuthResponse"/> packets.
    /// </summary>
    [JsonPropertyName("ok")]
    public bool Success { get; set; }

    /// <summary>Optional message (e.g. rejection reason).</summary>
    [JsonPropertyName("msg")]
    public string Message { get; set; } = string.Empty;

    // ─── Serialization ───

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public byte[] ToBytes() => JsonSerializer.SerializeToUtf8Bytes(this, s_jsonOptions);

    public static AuthPacket FromBytes(byte[] data) =>
        JsonSerializer.Deserialize<AuthPacket>(data, s_jsonOptions)
        ?? throw new InvalidDataException("Failed to deserialize AuthPacket.");

    /// <summary>Builds a client → server authentication request packet.</summary>
    public static byte[] BuildAuthRequest(string pin)
    {
        var packet = new AuthPacket { Pin = pin };
        return PacketBuilder.Build(DataType.Authentication, packet.ToBytes());
    }

    /// <summary>Builds a server → client authentication response packet.</summary>
    public static byte[] BuildAuthResponse(bool success, string message = "")
    {
        var packet = new AuthPacket { Success = success, Message = message };
        return PacketBuilder.Build(DataType.AuthResponse, packet.ToBytes());
    }
}
