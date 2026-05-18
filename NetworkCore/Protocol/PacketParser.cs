namespace NetworkCore.Protocol;

/// <summary>
/// Parses incoming packets by splitting the 1-byte <see cref="DataType"/> header
/// from the remaining payload.
/// </summary>
public static class PacketParser
{
    /// <summary>
    /// The result of parsing a raw packet: the extracted type tag and payload.
    /// </summary>
    /// <param name="Type">The data type tag extracted from byte 0.</param>
    /// <param name="Payload">The payload bytes (everything after byte 0).</param>
    public readonly record struct ParsedPacket(DataType Type, byte[] Payload);

    /// <summary>
    /// Splits the raw packet into its <see cref="DataType"/> tag and payload.
    /// </summary>
    /// <param name="rawPacket">
    /// The complete packet received from AsyncTcp (after length-prefix removal).
    /// Must be at least 1 byte.
    /// </param>
    /// <returns>A <see cref="ParsedPacket"/> containing the type and payload.</returns>
    /// <exception cref="ArgumentException">Thrown when the packet is empty.</exception>
    public static ParsedPacket Parse(byte[] rawPacket)
    {
        ArgumentNullException.ThrowIfNull(rawPacket);

        if (rawPacket.Length < 1)
            throw new ArgumentException("Packet must contain at least 1 byte (the DataType header).", nameof(rawPacket));

        var type = (DataType)rawPacket[0];
        var payload = new byte[rawPacket.Length - 1];

        if (payload.Length > 0)
            Buffer.BlockCopy(rawPacket, 1, payload, 0, payload.Length);

        return new ParsedPacket(type, payload);
    }

    /// <summary>
    /// Convenience: parses the packet and decodes the payload as a UTF-8 string.
    /// Only valid when <see cref="ParsedPacket.Type"/> is <see cref="DataType.Text"/>.
    /// </summary>
    public static string ParseText(byte[] rawPacket)
    {
        var parsed = Parse(rawPacket);
        return System.Text.Encoding.UTF8.GetString(parsed.Payload);
    }

    /// <summary>
    /// Convenience: parses the packet and deserializes the payload as a <see cref="ChatPacket"/>.
    /// Only valid when <see cref="ParsedPacket.Type"/> is <see cref="DataType.Chat"/>.
    /// </summary>
    public static ChatPacket ParseChat(byte[] payload)
    {
        return ChatPacket.FromBytes(payload);
    }
}
