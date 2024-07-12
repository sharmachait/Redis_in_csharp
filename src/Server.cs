using System.Net;
using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis
{
    class TcpServer
    {
        private readonly TcpListener _server;
        private Dictionary<int, Socket> connectedSockets = new Dictionary<int, Socket>();
        private int c = 0;
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
                HandleSocketAsync(socket);
            }
        }

        async Task HandleSocketAsync(Socket clientSocket)
        {
            while (clientSocket.Connected)
            {
                byte[] command = new byte[clientSocket.ReceiveBufferSize];
                await clientSocket.ReceiveAsync(command);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("+PONG\r\n"));
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            TcpServer server = new TcpServer(IPAddress.Any, 6379);
            await server.StartAsync();
        }
    }
}
