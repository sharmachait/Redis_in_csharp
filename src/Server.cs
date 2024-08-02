using Microsoft.Extensions.DependencyInjection;

namespace codecrafters_redis;

class Program
{
    static async Task Main(string[] args)
    {
        int portFlag = args.ToList().IndexOf("--port");
        int replicaFlag = args.ToList().IndexOf("--replicaof");
        ServiceCollection serviceProvider = new ServiceCollection();

        TcpServer server;


        if (portFlag > -1) 
        {
            int port = int.Parse(args[portFlag + 1]);

            if (replicaFlag > -1) 
            {
                string masterHost = args[replicaFlag + 1].Split(' ')[0];
                int masterPort = int.Parse(args[replicaFlag + 1].Split(' ')[1]);

                RedisConfig config = new RedisConfig("slave", port, masterPort, masterHost);

                server = new TcpServer(config);
            }
            else
            {
                RedisConfig config = new RedisConfig(port);

                server = new TcpServer(config);
            }
        }
        else 
        {
            RedisConfig config = new RedisConfig();

            server = new TcpServer(config);
        }
        await server.StartAsync();
    }
}

