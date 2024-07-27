using System.Net;

namespace codecrafters_redis;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine(args.Length);
        TcpServer server = new TcpServer(IPAddress.Any, 6379);
        await server.StartAsync();
    }
}

