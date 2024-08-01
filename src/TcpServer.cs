namespace codecrafters_redis;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TcpServer
{
    private readonly TcpListener _server;
    private readonly Store _store;

    public TcpServer(Config config)
    {
        _store = new Store();
        _store.role = role;
        if (role.Equals("slave"))
        {
            _store.MasterHost = args[3].Split(' ')[0];
            _store.MasterPort = args[3].Split(' ')[1];
        }
        _server = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync(string[] args)
    {
        _server.Start();
        Console.WriteLine("Server started..."+args.Length);

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
            String[] command = parser.GetCommand();
            Console.WriteLine("Command Parsed: ");
            foreach (String cmd in command)
            {
                Console.Write(cmd + " ");
            }
            Console.WriteLine();
            Console.WriteLine(_store.MasterHost);
            Console.WriteLine(_store.MasterPort);

            CommandHandler commandHandler = new CommandHandler(command,_store,parser);
            String response = commandHandler.GetResponse();
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(response));
        }
    }
}


