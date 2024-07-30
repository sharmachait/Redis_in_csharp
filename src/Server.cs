using System.Net;

namespace codecrafters_redis;

class Program
{
    static async Task Main(string[] args)
    {
        int portFlag = args.ToList().IndexOf("--port");
        int replicaFlag = args.ToList().IndexOf("--replicaof");
        if (portFlag > -1) 
        {
            int port = int.Parse(args[portFlag + 1]);
            if (replicaFlag > -1) 
            {
                
                TcpServer server = new TcpServer(IPAddress.Any, port, "slave", args);
                await server.StartAsync(args);
            }
            else
            {
                TcpServer server = new TcpServer(IPAddress.Any, port, "master", args);
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

