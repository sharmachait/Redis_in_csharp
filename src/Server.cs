using System.Net;
using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis
{
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
                Socket clientSocket = await _server.AcceptSocketAsync();
                await ClientHandler(clientSocket);
            }
        }

        public async Task ClientHandler(Socket clientSocket)
        {
            while (clientSocket.Connected)
            {
                byte[] buffer = new byte[1024];
                await clientSocket.ReceiveAsync(buffer);
                await clientSocket.SendAsync(Encoding.ASCII.GetBytes("+PONG\r\n"));
            }
            clientSocket.Dispose();
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
