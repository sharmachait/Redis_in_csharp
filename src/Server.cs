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
                string masterHost = args[replicaFlag + 1].Split(' ')[0];
                int masterPort = int.Parse(args[replicaFlag + 1].Split(' ')[1]);

                RedisConfig config = new RedisConfig("slave", port, masterPort, masterHost);

                TcpServer server = new TcpServer(config);
                await server.StartAsync(args);
            }
            else
            {
                RedisConfig config = new RedisConfig(port);

                TcpServer server = new TcpServer(config);
                await server.StartAsync(args);
            }
        }
        else 
        {
            RedisConfig config = new RedisConfig();

            TcpServer server = new TcpServer(config);
            await server.StartAsync(args);
        }
    }
}

