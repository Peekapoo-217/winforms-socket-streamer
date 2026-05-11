using NetworkCore.Protocol;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace SharpView.Relay;

/// <summary>
/// Represents a registered Host waiting for a Viewer to connect.
/// </summary>
sealed class HostSession
{
    public required string Password { get; init; }
    public required TcpClient Client { get; init; }
}

class Program
{
    // Key = Partner ID (9-digit string), Value = Host session info
    private static readonly ConcurrentDictionary<string, HostSession> _waitingHosts = new();

    static async Task Main(string[] args)
    {
        int port = 9000;
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine($"[SharpView.Relay] Started on port {port}. Waiting for connections...");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            Console.WriteLine($"[+] Client connected from {client.Client.RemoteEndPoint}");
            _ = HandleClientAsync(client);
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();

            // Wait for the initial handshake packet
            var lengthBytes = await ReadExactAsync(stream, 4);
            if (lengthBytes == null) return;

            var frameLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBytes, 0));
            var payload = await ReadExactAsync(stream, frameLength);
            if (payload == null) return;

            var packet = PacketParser.Parse(payload);

            if (packet.Type == DataType.RegisterHost)
            {
                // ─── HOST REGISTRATION ───
                var reg = SessionPacket.FromBytes(packet.Payload);
                string partnerId = reg.PartnerId;
                string password = reg.Password;

                // Store the Host keyed by its Partner ID
                _waitingHosts[partnerId] = new HostSession
                {
                    Password = password,
                    Client = client
                };

                Console.WriteLine($"[HOST] Registered Partner ID: {partnerId} (password: {password})");

                // Confirm registration back to the Host
                var response = SessionPacket.BuildHostRegistered(partnerId, "Host registered successfully.");
                await SendFrameAsync(stream, response);

                // The host will sit here waiting to be bridged...
            }
            else if (packet.Type == DataType.JoinSession)
            {
                // ─── VIEWER JOINING ───
                var join = SessionPacket.FromBytes(packet.Payload);
                string partnerId = join.PartnerId;
                string password = join.Password;

                Console.WriteLine($"[VIEWER] Attempting to join Partner ID: {partnerId}");

                // Step 1: Check if Partner ID exists
                if (!_waitingHosts.TryGetValue(partnerId, out var hostSession))
                {
                    Console.WriteLine($"[ERROR] Partner ID not found: {partnerId}");
                    var errorPacket = SessionPacket.BuildSessionError(partnerId, "Partner ID not found. The host may be offline.");
                    await SendFrameAsync(stream, errorPacket);
                    client.Close();
                    return;
                }

                // Step 2: Verify password
                if (hostSession.Password != password)
                {
                    Console.WriteLine($"[ERROR] Wrong password for Partner ID: {partnerId}");
                    var errorPacket = SessionPacket.BuildSessionError(partnerId, "Incorrect password.");
                    await SendFrameAsync(stream, errorPacket);
                    client.Close();
                    return;
                }

                // Step 3: Remove from waiting pool (one-time pairing)
                _waitingHosts.TryRemove(partnerId, out _);

                Console.WriteLine($"[PAIRING] Matched Partner ID {partnerId}. Starting bridge...");

                // Notify both parties
                var pairedPacket = SessionPacket.BuildSessionPaired(partnerId, "Session paired successfully.");
                await SendFrameAsync(stream, pairedPacket);                     // To Viewer
                await SendFrameAsync(hostSession.Client.GetStream(), pairedPacket); // To Host

                // Start bidirectional bridging
                await BridgeStreamsAsync(client, hostSession.Client, partnerId);
            }
            else
            {
                Console.WriteLine($"[WARN] Invalid initial packet type: {packet.Type}");
                client.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] HandleClient: {ex.Message}");
            client.Close();
        }
    }

    private static async Task BridgeStreamsAsync(TcpClient viewerClient, TcpClient hostClient, string partnerId)
    {
        using (viewerClient)
        using (hostClient)
        {
            var viewerStream = viewerClient.GetStream();
            var hostStream = hostClient.GetStream();

            var t1 = viewerStream.CopyToAsync(hostStream);
            var t2 = hostStream.CopyToAsync(viewerStream);

            try
            {
                await Task.WhenAny(t1, t2);
            }
            catch { /* Ignored */ }

            Console.WriteLine($"[DISCONNECT] Session for Partner ID {partnerId} ended.");
        }
    }

    private static async Task<byte[]?> ReadExactAsync(NetworkStream stream, int count)
    {
        var buffer = new byte[count];
        var offset = 0;
        while (offset < count)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset));
            if (read == 0) return null;
            offset += read;
        }
        return buffer;
    }

    private static async Task SendFrameAsync(NetworkStream stream, byte[] data)
    {
        var header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
        await stream.WriteAsync(header);
        await stream.WriteAsync(data);
        await stream.FlushAsync();
    }
}
