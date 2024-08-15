using codecrafters_redis.src;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using System.Text;

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
            .AddSingleton<Infra>()
            .AddSingleton<RespParser>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<TcpServer>()
            .BuildServiceProvider();

        TcpServer app = serviceProvider.GetRequiredService<TcpServer>();

       _ = Task.Run(() => app.Start());

        if (config.role.Equals("slave"))
        {
            TcpClient? ConnectionWithMaster = await app.InitiateSlaveryAsync(
                config.port, config.masterHost, config.masterPort
            );
            if (ConnectionWithMaster == null)
            {
                Console.WriteLine("Connection not established with master, please retry");
                return;
            }
            else
            {
                //start receiving from master on different thread
                _ = Task.Run(async () => await app.StartMasterPropagation(ConnectionWithMaster));
            }
        }
    }
}

