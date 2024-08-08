using System.Net.Sockets;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.IO;

namespace codecrafters_redis.src;

public class Infra
{
    public List<Slave> slaves =  new List<Slave>();
    public List<Client> clients = new List<Client>();


}

public class Slave
{
    public int port;
    public string ipaddress;
    public List<string> capabilities;
    public Slave(int port, string ipaddress)
    {
        this.port = port;
        this.ipaddress = ipaddress;
        capabilities = new List<string>();
    }
}

public class Client
{
    public TcpClient socket;
    public IPEndPoint remoteIpEndPoint;
    public NetworkStream stream;
    string clientIpAddress;
    int clientPort;
    int id;

    public Client(TcpClient socket, IPEndPoint ip, NetworkStream stream, int id)
    {
        this.socket = socket;
        remoteIpEndPoint = ip;
        this.stream = stream;
        clientIpAddress = remoteIpEndPoint.Address.ToString();
        this.id = id;
        clientPort = remoteIpEndPoint.Port;
    }

    public async Task SendAsync(string response)
    {
        Console.WriteLine(Encoding.UTF8.GetBytes(response));
        await this.stream.WriteAsync(Encoding.UTF8.GetBytes(response));
    }
}