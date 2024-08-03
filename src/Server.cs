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
                using (TcpClient client = new TcpClient(masterHost, masterPort))
                {
                    NetworkStream stream = client.GetStream();
                    string ping = "*1\r\n$4\r\nPING\r\n";
                    stream.Write(Encoding.UTF8.GetBytes(ping));
                    string ReplconfPort = $"*3\r\n$8\r\nREPLCONF\r\n$14\r\nlistening-port\r\n$4\r\n{config.port}\r\n";
                    StreamReader reader = new StreamReader(stream,Encoding.UTF8);
                    Console.WriteLine("Response: " + reader.ReadToEnd());
                }
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

