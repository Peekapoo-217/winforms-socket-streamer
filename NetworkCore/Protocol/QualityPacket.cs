namespace NetworkCore.Protocol;

/// <summary>
/// Binary payload for Dynamic Quality Scaling commands (Viewer → Host).
/// Fixed 8 bytes: [JpegQuality: int32][StreamIntervalMs: int32].
/// </summary>
public class QualityPacket
{
    public int JpegQuality { get; set; }
    public int StreamIntervalMs { get; set; }

    public byte[] ToBytes()
    {
        var bytes = new byte[8];
        Buffer.BlockCopy(BitConverter.GetBytes(JpegQuality), 0, bytes, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(StreamIntervalMs), 0, bytes, 4, 4);
        return bytes;
    }

    public static QualityPacket FromBytes(byte[] payload)
    {
        return new QualityPacket
        {
            JpegQuality = BitConverter.ToInt32(payload, 0),
            StreamIntervalMs = BitConverter.ToInt32(payload, 4)
        };
    }

    /// <summary>Builds a tagged packet ready to send over the wire.</summary>
    public byte[] BuildPacket()
    {
        return PacketBuilder.Build(DataType.QualityCommand, ToBytes());
    }
}
