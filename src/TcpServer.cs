namespace codecrafters_redis;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TcpServer
{
    private readonly TcpListener _server;

    public TcpServer(IPAddress ipAddress, int port)
    {
        _server = new TcpListener(ipAddress, port);
    }

    public async Task StartAsync()
    {
        _server.Start();
        Console.WriteLine("Server started...");

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

            CommandHandler commandHandler = new CommandHandler(command);
            String response = commandHandler.GetResponse();
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(response));
        }
    }
}
