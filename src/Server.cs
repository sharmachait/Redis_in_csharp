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
                var clientSocket = await _server.AcceptSocketAsync();
                while (clientSocket.Connected)
                {
                    var buffer = new byte[1024];
                    var byteCount = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                    if (byteCount > 0)
                    {
                        Console.WriteLine("Received data from client");
                        await clientSocket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes("+PONG\r\n")), SocketFlags.None);
                    }
                    else
                    {
                        Console.WriteLine("Client disconnected");
                        clientSocket.Close();
                    }
                }
            }
        }

        public void Stop()
        {
            _server.Stop();
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