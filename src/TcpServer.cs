namespace codecrafters_redis;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TcpServer
{
    private readonly TcpListener _server;
    private readonly Store _store;

    public TcpServer(RedisConfig config)
    {
        _store = new Store();
        _store.role = config.role;
        if (config.role.Equals("slave"))
        {
            _store.MasterHost = config.masterHost;
            _store.MasterPort = config.masterPort;
        }
        _server = new TcpListener(IPAddress.Any, config.port);
    }

    public async Task StartAsync()
    {
        _server.Start();
        Console.WriteLine("Server started..." );

        while (true)
        {
            Socket socket = await _server.AcceptSocketAsync();
            HandleClientAsync(socket);
        }
    }

    async Task HandleClientAsync(Socket clientSocket)
    {
        while (clientSocket.Connected)
        {
            byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
            await clientSocket.ReceiveAsync(buffer);

            RespParser parser = new RespParser(buffer);
            string[] command = parser.GetCommand();
            Console.WriteLine("Command Parsed: ");
            foreach (string cmd in command)
            {
                Console.Write(cmd + " ");
            }
            Console.WriteLine();
            Console.WriteLine(_store.MasterHost);
            Console.WriteLine(_store.MasterPort);

            CommandHandler commandHandler = new CommandHandler(command,_store,parser);
            string response = commandHandler.GetResponse();
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(response));
        }
    }
}


