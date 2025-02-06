using System.Net;
using System.Net.Sockets;

internal class Program
{
    static List<Connection> connectedClients = new List<Connection>();
    static Connection currentStreamer;
    static byte[] tempBuffer;
    private static async Task Main(string[] args)
    {
        var tasks = new List<Func<Task>>
        {
           StartListening,
           ReadDataFromStreamer,
           ShowDetails
        };

        await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
    }

    async static Task StartListening()
    {
        var serverEP = new IPEndPoint(IPAddress.Any, 51333);
        using (TcpListener listener = new TcpListener(serverEP))
        {
            listener.Start();
            while (true)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();
                NetworkStream nwStream = tcpClient.GetStream();
                if (!connectedClients.Any(f => f.Client.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint))
                {
                    connectedClients.Add(new Connection
                    {
                        Id = Guid.NewGuid(),
                        Client = tcpClient,
                        Type = !connectedClients.Any() ? ConnectionType.Streamer : ConnectionType.Client,
                    });
                }
            }
        }
    }

    async static Task ReadDataFromStreamer()
    {
        while (true)
        {
            try
            {
                currentStreamer = connectedClients.FirstOrDefault(f => f.Type == ConnectionType.Streamer);
                if (currentStreamer == null)
                {
                    Task.Delay(1000).Wait();
                    continue;
                }

                tempBuffer = new byte[currentStreamer.Client.ReceiveBufferSize];
                var stream = currentStreamer.Client.GetStream();
                stream.Read(tempBuffer, 0, currentStreamer.Client.ReceiveBufferSize);

                foreach (var client in connectedClients.Where(f => f.Type == ConnectionType.Client))
                {
                    client.Client.GetStream().Write(tempBuffer, 0, currentStreamer.Client.ReceiveBufferSize);
                }
            }
            catch (Exception)
            {
                Task.Delay(1000).Wait();
                continue;
            }
        }
    }

    async static Task ShowDetails()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("Streamer: {0}", (connectedClients.Any(f => f.Type == ConnectionType.Streamer) ? "Connected" : "Not Connected"));
            Console.WriteLine("Clients: {0}", connectedClients.Count(f => f.Type == ConnectionType.Client));
            Console.WriteLine("CurrentByteCount: {0}", (tempBuffer != null ? tempBuffer.Length : 0));
            Task.Delay(1000).Wait();
        }
    }
}

public class Connection
{
    public Guid Id { get; set; }

    public ConnectionType Type { get; set; }

    public TcpClient Client { get; set; }
}

public enum ConnectionType
{
    Client,
    Streamer
}