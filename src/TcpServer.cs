namespace codecrafters_redis;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TcpServer
{
    private readonly TcpListener _server;
    private readonly RespParser _parser ;
    private readonly CommandHandler _handler;
    private readonly RedisConfig _config;

    public TcpServer(RedisConfig config, Store store, RespParser parser, CommandHandler handler)
    {
        _handler = handler;

        _parser = parser;

        _config = config;

        _server = new TcpListener(IPAddress.Any, config.port);
    }

    public async Task StartAsync()
    {
        _server.Start();
        Console.WriteLine("Server started..." );

        while (true)
        {
            Socket socket = await _server.AcceptSocketAsync();
            
            TcpClient ConnectedClient = _server.AcceptTcpClient();
            IPEndPoint remoteIpEndPoint = ConnectedClient.Client.RemoteEndPoint as IPEndPoint;
            string clientIpAddress = remoteIpEndPoint.Address.ToString();

            Console.WriteLine("clientIpAddress: => " + clientIpAddress);

            _ = Task.Run(() => HandleClientAsync(socket, clientIpAddress));
        }
    }
    async Task HandleClientAsync(Socket clientSocket, string clientIpAddress)
    {
        using (clientSocket)
            while (clientSocket.Connected)
            {
                byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
                await clientSocket.ReceiveAsync(buffer);

                string[] command = _parser.MakeCommand(buffer);

                Console.WriteLine("Command Parsed: ");
                foreach (string cmd in command)
                {
                    Console.Write(cmd + " ");
                }

                string response = _handler.Handle(command, clientIpAddress);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes(response));
            }
        //implement exponential backoff if disconnected

        //polymorphism?
    }

    public async Task<TcpClient?> InitiateSlaveryAsync(int port, string masterHost, int masterPort)
    {
        TcpClient client = new TcpClient(masterHost, masterPort);

        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream, Encoding.UTF8);

        string[] pingCommand = ["PING"];

        stream.Write(Encoding.UTF8.GetBytes(_parser.RespArray(pingCommand)));

        string response = reader.ReadLine();

        if (!"+PONG".Equals(response))
            return null;



        string[] ReplconfPortCommand = ["REPLCONF", "listening-port", _config.port.ToString()];

        stream.Write(Encoding.UTF8.GetBytes(_parser.RespArray(ReplconfPortCommand)));

        response = reader.ReadLine();

        if (!"+OK".Equals(response))
            return null;



        string[] ReplconfCapaCommand = ["REPLCONF", "capa", "psync2"];

        stream.Write(Encoding.UTF8.GetBytes(_parser.RespArray(ReplconfCapaCommand)));

        response = reader.ReadLine();

        if (!"+OK".Equals(response))
            return null;



        string[] PsyncCommand = ["PSYNC", "?", "-1"];

        stream.Write(Encoding.UTF8.GetBytes(_parser.RespArray(PsyncCommand)));

        response = reader.ReadLine();

        if (response == null || !"+FULLRESYNC".Equals(response.Substring(0, response.IndexOf(" "))))
        {
            return null;
        }

        return client;
        
    }
}


