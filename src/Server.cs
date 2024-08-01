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
                string MasterHost = args[3].Split(' ')[0];
                int MasterPort = args[3].Split(' ')[1];


                Config config new Config(config);

                TcpServer server = new TcpServer(config);
                await server.StartAsync(args);
            }
            else
            {
                Config config new Config(port);

                TcpServer server = new TcpServer(config);
                await server.StartAsync(args);
            }
        }
        else 
        {

            Config config new Config();

            TcpServer server = new TcpServer(config);
            await server.StartAsync(args);
        }
    }
}

/*public Config(string role, int port, int masterPort, string masterHost)
{
    this.role = role;
    this.port = port;
    this.masterHost = masterHost;
    this.masterPort = masterPort;
}

public Config(int port)
{
    this.role = "master";
    this.port = port;
    this.masterHost = ".";
    this.masterPort = int.MinValue;
}

public Config()
{
    this.role = "slave";
    this.port = port;
    this.masterHost = "NA";
    this.masterPort = 6379;
}*/