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

            while (true)
            {
                Socket clientSocket = await _server.AcceptSocketAsync();
                await HandleSocketAsync(clientSocket);
            }
        }

        async Task HandleSocketAsync(Socket clientSocket)
        {
            while (clientSocket.Connected)
            {
                var command = new byte[clientSocket.ReceiveBufferSize];
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
