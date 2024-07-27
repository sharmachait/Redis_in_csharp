namespace codecrafters_redis;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TcpServer
{
    private readonly TcpListener _server;
    private readonly Store _store;
    public TcpServer(IPAddress ipAddress, int port)
    {
        _store = new Store();
        _server = new TcpListener(ipAddress, port);
    }

    public async Task StartAsync(string[] args)
    {
        _server.Start();
        Console.WriteLine("Server started...");

        if (args.Length > 2) {// --port port_number info replication
            Console.WriteLine(args[3]);
            RespParser parser = new RespParser();
            Console.WriteLine(parser.MakeBulkString(args[4]));
        }

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
            Console.WriteLine("Steps: ");

            CommandHandler commandHandler = new CommandHandler(command,_store);
            String response = commandHandler.GetResponse();
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(response));
        }
    }
}


