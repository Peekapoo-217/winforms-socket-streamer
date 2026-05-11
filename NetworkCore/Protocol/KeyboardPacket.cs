using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetworkCore.Protocol;

/// <summary>
/// Lightweight payload representing a keyboard input event to be sent over the network.
/// Serialized to JSON → byte[] via <see cref="PacketBuilder"/>.
/// </summary>
public sealed class KeyboardPacket
{
    /// <summary>
    /// The key identifier. Use <see cref="System.Windows.Forms.Keys"/> enum name
    /// (e.g. "A", "Enter", "ControlKey", "ShiftKey", "F5").
    /// </summary>
    [JsonPropertyName("key")]
    public string KeyCode { get; set; } = string.Empty;

    /// <summary>
    /// <c>true</c> = key pressed down; <c>false</c> = key released.
    /// </summary>
    [JsonPropertyName("down")]
    public bool IsDown { get; set; }

    // ─── Serialization helpers ───

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Serializes this packet to a UTF-8 JSON byte array.</summary>
    public byte[] ToBytes() => JsonSerializer.SerializeToUtf8Bytes(this, s_jsonOptions);

    /// <summary>Deserializes a UTF-8 JSON byte array back into a <see cref="KeyboardPacket"/>.</summary>
    public static KeyboardPacket FromBytes(byte[] data) =>
        JsonSerializer.Deserialize<KeyboardPacket>(data, s_jsonOptions)
        ?? throw new InvalidDataException("Failed to deserialize KeyboardPacket.");

    /// <summary>Convenience: wraps this packet with <see cref="DataType.KeyboardCommand"/> for sending.</summary>
    public byte[] BuildPacket() => PacketBuilder.Build(DataType.KeyboardCommand, ToBytes());
}
