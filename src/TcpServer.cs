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
            byte[] command = new byte[clientSocket.ReceiveBufferSize];
            await clientSocket.ReceiveAsync(command);

            RespParser parser = new RespParser(command);
            String[] _command = parser.GetCommand();
            Console.WriteLine("Command: ");

            for (int i = 0; i < _command.Length; i++)
            {
                Console.WriteLine(i+": "+_command[i]);
            }
            // await clientSocket.SendAsync(Encoding.UTF8.GetBytes("+PONG\r\n"));
        }
    }
}
