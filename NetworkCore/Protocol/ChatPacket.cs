using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetworkCore.Protocol;

/// <summary>
/// Payload for bidirectional chat messages between Host and Viewer.
/// Serialized as JSON, following the same pattern as <see cref="MousePacket"/>.
/// </summary>
public sealed class ChatPacket
{
    /// <summary>Display name of the message sender (e.g. "Host", "Viewer").</summary>
    [JsonPropertyName("senderName")]
    public string SenderName { get; set; } = string.Empty;

    /// <summary>The chat message content.</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the message was created.</summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // ─── Serialization helpers ───

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    /// <summary>Serializes this packet to a UTF-8 JSON byte array.</summary>
    public byte[] ToBytes() => JsonSerializer.SerializeToUtf8Bytes(this, s_jsonOptions);

    /// <summary>Deserializes a UTF-8 JSON byte array back into a <see cref="ChatPacket"/>.</summary>
    public static ChatPacket FromBytes(byte[] data) =>
        JsonSerializer.Deserialize<ChatPacket>(data, s_jsonOptions)
        ?? throw new InvalidDataException("Failed to deserialize ChatPacket.");

    /// <summary>Convenience: wraps this packet with <see cref="DataType.Chat"/> for sending.</summary>
    public byte[] BuildPacket() => PacketBuilder.Build(DataType.Chat, ToBytes());
}
