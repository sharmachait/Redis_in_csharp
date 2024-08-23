using codecrafters_redis.src;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;

namespace codecrafters_redis;

class Program
{
    static async Task Main(string[] args)
    {
        RedisConfig config = new RedisConfig() ;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--port":
                    config.port = int.Parse(args[i + 1]);
                    break;
                case "--replicaof":
                    config.role = "slave";
                    string masterHost = args[i + 1].Split(' ')[0];
                    int masterPort = int.Parse(args[i + 1].Split(' ')[1]);
                    config.masterHost = masterHost;
                    config.masterPort = masterPort;
                    break;
                default:
                    break;
            }
        }


        //int portFlag = args.ToList().IndexOf("--port");
        //int replicaFlag = args.ToList().IndexOf("--replicaof");

        ////RedisConfig config;

        //if (portFlag > -1)
        //{
        //    int port = int.Parse(args[portFlag + 1]);

        //    if (replicaFlag > -1)
        //    {
        //        string masterHost = args[replicaFlag + 1].Split(' ')[0];

        //        int masterPort = int.Parse(args[replicaFlag + 1].Split(' ')[1]);

        //        config = new RedisConfig("slave", port, masterPort, masterHost);
        //    }
        //    else
        //    {
        //        config = new RedisConfig(port);
        //    }
        //}
        //else
        //{
        //    config = new RedisConfig();
        //}


        var serviceProvider = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<Store>()
            .AddSingleton<Infra>()
            .AddSingleton<RespParser>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<TcpServer>()
            .BuildServiceProvider();

        TcpServer app = serviceProvider.GetRequiredService<TcpServer>();

        if (config.role == "slave")
        {
            _ = Task.Run(async () => { await app.StartReplicaAsync(); });
        }
        ///////////////////////----------------------------------------aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
        try
        {
            await app.StartAsync();
        }
        finally
        {
            app._server.Stop();
        }


    }

}

