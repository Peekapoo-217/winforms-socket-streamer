namespace NetworkCore.Protocol;

/// <summary>
/// Builds outgoing packets by prepending a 1-byte <see cref="DataType"/> header
/// to the raw payload.
///
/// <para><b>Wire format:</b>
/// <code>
/// ┌──────────┬──────────────────────┐
/// │ DataType │      Payload         │
/// │ (1 byte) │    (N bytes)         │
/// └──────────┴──────────────────────┘
/// </code>
/// This sits inside the length-prefixed frame handled by AsyncTcp*.
/// </para>
/// </summary>
public static class PacketBuilder
{
    /// <summary>
    /// Creates a tagged packet by prepending the <paramref name="type"/> byte
    /// to <paramref name="payload"/>.
    /// </summary>
    /// <param name="type">The data type tag.</param>
    /// <param name="payload">The raw payload bytes (image, text, command…).</param>
    /// <returns>A new byte array: [DataType (1 byte)] + [Payload (N bytes)].</returns>
    public static byte[] Build(DataType type, byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var packet = new byte[1 + payload.Length];
        packet[0] = (byte)type;
        Buffer.BlockCopy(payload, 0, packet, 1, payload.Length);
        return packet;
    }

    /// <summary>
    /// Convenience overload: encodes a UTF-8 string as a <see cref="DataType.Text"/> packet.
    /// </summary>
    public static byte[] BuildText(string message)
    {
        var payload = System.Text.Encoding.UTF8.GetBytes(message);
        return Build(DataType.Text, payload);
    }

    /// <summary>
    /// Convenience overload: serializes a <see cref="ChatPacket"/> as a <see cref="DataType.Chat"/> packet.
    /// </summary>
    public static byte[] BuildChat(ChatPacket chatPacket)
    {
        ArgumentNullException.ThrowIfNull(chatPacket);
        return Build(DataType.Chat, chatPacket.ToBytes());
    }
}
