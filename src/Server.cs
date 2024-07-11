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

        public async Task StartAsyncDoesntWork()
        {
            _server.Start();
            while (true)
            {
                Socket clientSocket =await _server.AcceptSocketAsync();
                while (clientSocket.Connected)
                {
                    byte[] buffer = new byte[1024];
                    await clientSocket.ReceiveAsync(buffer);
                    await clientSocket.SendAsync(Encoding.ASCII.GetBytes("+PONG\r\n"));
                }
            }
        }

        public async Task StartAsync()
        {
            _server.Start();
            Console.WriteLine("Server started...");

            while (true)
            {
                Socket clientSocket = await _server.AcceptSocketAsync();
                while (clientSocket.Connected)
                {
                    byte[] buffer = new byte[1024];
                    await clientSocket.ReceiveAsync(buffer);
                    await clientSocket.SendAsync(Encoding.ASCII.GetBytes("+PONG\r\n"));
                }
            }
        }


    }

    class Program
    {
        static async Task Main(string[] args)
        {
            TcpServer server = new TcpServer(IPAddress.Any, 6379);
            await server.StartAsyncDoesntWork();
        }
    }
}
