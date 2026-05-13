namespace NetworkCore.Protocol;

/// <summary>
/// Binary payload for Live System Monitor telemetry (Host → Viewer).
/// Fixed 8 bytes: [CpuUsage: float][RamUsagePercentage: float].
/// </summary>
public class MonitorPacket
{
    /// <summary>CPU usage percentage (0.0 – 100.0).</summary>
    public float CpuUsage { get; set; }

    /// <summary>RAM usage percentage (0.0 – 100.0).</summary>
    public float RamUsagePercentage { get; set; }

    public byte[] ToBytes()
    {
        var bytes = new byte[8];
        Buffer.BlockCopy(BitConverter.GetBytes(CpuUsage), 0, bytes, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(RamUsagePercentage), 0, bytes, 4, 4);
        return bytes;
    }

    public static MonitorPacket FromBytes(byte[] payload)
    {
        return new MonitorPacket
        {
            CpuUsage = BitConverter.ToSingle(payload, 0),
            RamUsagePercentage = BitConverter.ToSingle(payload, 4)
        };
    }

    /// <summary>Builds a tagged packet ready to send over the wire.</summary>
    public byte[] BuildPacket()
    {
        return PacketBuilder.Build(DataType.SystemMonitor, ToBytes());
    }
}
