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

                config = InitiateSlavery( port, masterHost, masterPort);
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

    static RedisConfig InitiateSlavery(int port, string masterHost,int masterPort) { 
        RespParser parser=new RespParser();
        RedisConfig config = new RedisConfig("slave", port, masterPort, masterHost);
        using (TcpClient client = new TcpClient(masterHost, masterPort))
        {
            NetworkStream stream = client.GetStream();

            string[] ping =["PING"];
            stream.WriteAsync(Encoding.UTF8.GetBytes(parser.RespArray(ping)));
            //StreamReader reader = new StreamReader(stream,Encoding.UTF8);
            //Console.WriteLine("Response from master: " + reader.ReadToEnd());//+PONG

            //string[] ReplconfPort = ["REPLCONF", "listening-port", config.port.ToString()];

            //stream.WriteAsync(Encoding.UTF8.GetBytes(parser.RespArray(ReplconfPort)));
            ////StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            ////Console.WriteLine("Response from master: " + reader.ReadToEnd());//+OK

            //string[] ReplconfCapa = ["REPLCONF", "capa", "psync2"];
            //stream.WriteAsync(Encoding.UTF8.GetBytes(parser.RespArray(ReplconfCapa)));
            ////StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            ////Console.WriteLine("Response from master: " + reader.ReadToEnd());//+OK
        }
        return config;

    }
}

