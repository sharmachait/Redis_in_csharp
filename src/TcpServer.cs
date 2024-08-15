namespace codecrafters_redis;

using codecrafters_redis.src;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TcpServer
{
    private readonly TcpListener _server;
    private readonly RespParser _parser;
    private readonly CommandHandler _handler;
    private readonly RedisConfig _config;
    private readonly Infra _infra;
    private int id;


    public TcpServer(RedisConfig config, Store store, RespParser parser, CommandHandler handler, Infra infra)
    {
        _handler = handler;

        _parser = parser;

        _config = config;

        _infra = infra;

        id = 0;

        _server = new TcpListener(IPAddress.Any, config.port);
    }


    public void Start()
    {
        _server.Start();

        Console.WriteLine("Server started...");

        while (true)
        {
            TcpClient socket = _server.AcceptTcpClient();
            id++;
            IPEndPoint? remoteIpEndPoint = socket.Client.RemoteEndPoint as IPEndPoint;
            if (remoteIpEndPoint == null)
                return;

            NetworkStream stream = socket.GetStream();

            Client client = new Client(socket, remoteIpEndPoint, stream, id);

            _infra.clients.Add(client);

            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    public async Task StartMasterPropagation(TcpClient ConnectionWithMaster)
    {
        while (ConnectionWithMaster.Connected)
        {
            NetworkStream stream = ConnectionWithMaster.GetStream();

            byte[] buffer = new byte[ConnectionWithMaster.ReceiveBufferSize];

            stream.Read(buffer, 0, buffer.Length);

            List<string[]> commands = _parser.Deserialize(buffer);
            Console.WriteLine("commands from master: " + commands.Count + " **************************************************************************");

            foreach (string[] command in commands)
            {
                Console.WriteLine("commands from master: ");
                foreach (string c in command)
                {
                    Console.Write(c + " ");
                }
                Console.WriteLine();
                string response = await _handler.HandleMasterCommands(command);
            }
        }
    }

    public async Task HandleClientAsync(Client client)
    {

        while (client.socket.Connected)
        {
            byte[] buffer = new byte[client.socket.ReceiveBufferSize];

            await client.stream.ReadAsync(buffer);

            List<string[]> commands = _parser.Deserialize(buffer);

            foreach (string[] command in commands)
            {
                string response = await _handler.Handle(command, client);
                if (
                    client.ipAddress.Equals(_config.masterHost)
                    && client.port == _config.masterPort
                )
                {
                    //dont send to client when client writting to the instance is the master
                    return;
                }
                client.Send(response);
            }
        }

    }

    //done by slave instace
    //dont need to create the slave object here
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
        Console.WriteLine("replconf response" + " " + response + " ******************************************");

        if (!"+OK".Equals(response))
            return null;




        string[] ReplconfCapaCommand = ["REPLCONF", "capa", "psync2"];

        stream.Write(Encoding.UTF8.GetBytes(_parser.RespArray(ReplconfCapaCommand)));

        response = reader.ReadLine();
        Console.WriteLine("replconf response" + " " + response + " ******************************************");

        if (!"+OK".Equals(response))
            return null;




        string[] PsyncCommand = ["PSYNC", "?", "-1"];

        stream.Write(Encoding.UTF8.GetBytes(_parser.RespArray(PsyncCommand)));

        response = reader.ReadLine();
        Console.WriteLine("psync response" + " " + response + " ******************************************");

        if (response == null || !"+FULLRESYNC".Equals(response.Substring(0, response.IndexOf(" "))))
            return null;

        //do multi thread to listen from master
        Console.WriteLine("ready to process commands from master");
        return client;
    }
}


