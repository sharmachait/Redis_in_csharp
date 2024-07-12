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
                Socket clientSocket = await _server.AcceptSocketAsync();
                connectedSockets.Add(c++,clientSocket);
                await ClientHandler(c);
            }
        }

        public async Task ClientHandler(int key)
        {
            Socket clientSocket = connectedSockets[key];
            while (clientSocket.Connected)
            {
                byte[] buffer = new byte[1024];
                await clientSocket.ReceiveAsync(buffer);
                await clientSocket.SendAsync(Encoding.ASCII.GetBytes("+PONG\r\n"));
            }
            clientSocket.Dispose();
            connectedSockets.Remove(key);
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
