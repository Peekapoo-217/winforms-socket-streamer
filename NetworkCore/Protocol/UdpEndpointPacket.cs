using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetworkCore.Protocol;

/// <summary>
/// Payload sent via TCP (through Relay) to exchange UDP endpoint information
/// so that both peers can establish direct UDP communication for typing indicators.
/// </summary>
public sealed class UdpEndpointPacket
{
    /// <summary>The sender's LAN IP address string.</summary>
    [JsonPropertyName("ip")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>The sender's UDP listening port.</summary>
    [JsonPropertyName("udpPort")]
    public int UdpPort { get; set; }

    // ─── Serialization ───

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public byte[] ToBytes() => JsonSerializer.SerializeToUtf8Bytes(this, s_jsonOptions);

    public static UdpEndpointPacket FromBytes(byte[] data) =>
        JsonSerializer.Deserialize<UdpEndpointPacket>(data, s_jsonOptions)
        ?? throw new InvalidDataException("Failed to deserialize UdpEndpointPacket.");

    /// <summary>Builds a tagged packet with <see cref="DataType.UdpEndpoint"/>.</summary>
    public byte[] BuildPacket() => PacketBuilder.Build(DataType.UdpEndpoint, ToBytes());
}
