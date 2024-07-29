using System.Net;

namespace codecrafters_redis;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length > 0 && args[0].Equals("--port")) 
        {
            if (args.Length > 2 && args[2].Equals("--replicaof")) 
            {
                TcpServer server = new TcpServer(IPAddress.Any, int.Parse(args[1]), "slave",args);
                await server.StartAsync(args);
            }
            else
            {
                TcpServer server = new TcpServer(IPAddress.Any, int.Parse(args[1]), "master",args);
                await server.StartAsync(args);
            }
        }
        else 
        {
            TcpServer server = new TcpServer(IPAddress.Any, 6379, "master", args);
            await server.StartAsync(args);
        }
    }
}

