using System;
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

        public async Task Start()
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

        public void Stop()
        {
            _server.Stop();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TcpServer server = new TcpServer(IPAddress.Any, 6379);
            server.Start();
        }
    }
}