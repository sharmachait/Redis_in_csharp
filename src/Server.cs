using Microsoft.Extensions.DependencyInjection;

namespace codecrafters_redis;

class Program
{
    static async Task Main(string[] args)
    {
        int portFlag = args.ToList().IndexOf("--port");
        int replicaFlag = args.ToList().IndexOf("--replicaof");
        
        RedisConfig config;

        if (portFlag > -1) 
        {
            int port = int.Parse(args[portFlag + 1]);

            if (replicaFlag > -1) 
            {
                string masterHost = args[replicaFlag + 1].Split(' ')[0];
                int masterPort = int.Parse(args[replicaFlag + 1].Split(' ')[1]);

                config = new RedisConfig("slave", port, masterPort, masterHost);
            }
            else
            {
                config = new RedisConfig(port);
            }
        }
        else 
        {
            config = new RedisConfig();
        }
        

        var serviceProvider = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<Store>()
            .AddSingleton<RespParser>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<TcpServer>()
            .BuildServiceProvider();

        TcpServer app = serviceProvider.GetRequiredService<TcpServer>();
        await app.StartAsync();
    }
}

