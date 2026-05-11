using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetworkCore.Protocol;

/// <summary>
/// Lightweight payload representing a mouse input event to be sent over the network.
/// Serialized to JSON → byte[] via <see cref="PacketBuilder"/>.
/// </summary>
public sealed class MousePacket
{
    /// <summary>Absolute X coordinate on the server's screen.</summary>
    [JsonPropertyName("x")]
    public int X { get; set; }

    /// <summary>Absolute Y coordinate on the server's screen.</summary>
    [JsonPropertyName("y")]
    public int Y { get; set; }

    /// <summary>
    /// The mouse action to perform.
    /// Valid values: "Move", "LeftDown", "LeftUp", "LeftClick",
    /// "RightDown", "RightUp", "RightClick", "Scroll".
    /// </summary>
    [JsonPropertyName("action")]
    public string Action { get; set; } = "Move";

    /// <summary>
    /// Scroll delta (positive = scroll up, negative = scroll down).
    /// Only meaningful when <see cref="Action"/> is "Scroll".
    /// </summary>
    [JsonPropertyName("delta")]
    public int ScrollDelta { get; set; }

    // ─── Serialization helpers ───

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    /// <summary>Serializes this packet to a UTF-8 JSON byte array.</summary>
    public byte[] ToBytes() => JsonSerializer.SerializeToUtf8Bytes(this, s_jsonOptions);

    /// <summary>Deserializes a UTF-8 JSON byte array back into a <see cref="MousePacket"/>.</summary>
    public static MousePacket FromBytes(byte[] data) =>
        JsonSerializer.Deserialize<MousePacket>(data, s_jsonOptions)
        ?? throw new InvalidDataException("Failed to deserialize MousePacket.");

    /// <summary>Convenience: wraps this packet with <see cref="DataType.MouseCommand"/> for sending.</summary>
    public byte[] BuildPacket() => PacketBuilder.Build(DataType.MouseCommand, ToBytes());
}
