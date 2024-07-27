using System.Net;

namespace codecrafters_redis;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length > 0 && args[0].Equals("--port")) {
            TcpServer server = new TcpServer(IPAddress.Any, int.Parse(args[1]));
            await server.StartAsync(args);
        }
        else {
            TcpServer server = new TcpServer(IPAddress.Any, 6379);
            await server.StartAsync(args);
        }
    }
}

