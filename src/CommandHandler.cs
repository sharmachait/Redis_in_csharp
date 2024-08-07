using codecrafters_redis.src;
using System.Net;

namespace codecrafters_redis;

public class CommandHandler
{
    private readonly RespParser _parser;
    private readonly Store _store;
    private readonly RedisConfig _config;
    private readonly Infra _infra;

    public CommandHandler(Store store, RespParser parser, RedisConfig config, Infra infra)
    {
        _infra = infra;
        _parser = parser;
        _store = store;
        _config = config;
    }

    public string Handle(string[] command, IPEndPoint remoteIpEndPoint) {
        string cmd = command[0];
        DateTime currTime = DateTime.Now;

        switch (cmd)
        {
            case "ping":
                return "+PONG\r\n";
                
            case "echo":
                return $"+{command[1]}\r\n";
                
            case "get":
                return _store.Get(command, currTime);
                
            case "set":
                if (_config.role.Equals("slave"))
                {
                    string clientIpAddress = remoteIpEndPoint.Address.ToString();
                    int clientPort = remoteIpEndPoint.Port;

                    if (_config.masterHost.Equals(clientIpAddress))
                    {
                        return _store.Set(command, currTime);
                    }
                    else
                    {
                        return _parser.RespBulkString("READONLY You can't write against a read only replica.");
                    }
                }
                return _store.Set(command, currTime);

            case "info":
                return Info(command);

            case "replconf":
                return ReplConf(command, remoteIpEndPoint);
                
            default:
                return "+No Response\r\n";
                
        }
    }
    
    public string Info(string[] command)
    {
        switch (command[1])
        {
            case "replication":
                try
                {
                    return Replication();
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            default:
                return "Invalid options";
                
        }
    }

    public string ReplConf(string[] command, IPEndPoint remoteIpEndPoint)
    {

        string clientIpAddress = remoteIpEndPoint.Address.ToString();
        int clientPort = remoteIpEndPoint.Port;

        switch (command[1])
        {
            case "listening-port":
                try
                {
                    //Console.WriteLine("**************************************************************************************");
                    //Console.WriteLine("ClienPort " + clientPort);
                    //Console.WriteLine("listening port " + command[2]);
                    Slave s = new Slave(int.Parse(command[2]), clientIpAddress);
                    _infra.clients.Add(s);
                    return _parser.RespBulkString("OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return _parser.RespBulkString("NOTOK");
                }
            case "capa":
                
                int idx = _infra.clients.FindIndex((x) => { return x.ipaddress.Equals(clientIpAddress); });
                for (int i = 0; i < command.Length; i++) 
                {
                    if (command[i].Equals("capa"))
                    {
                        _infra.clients[idx].capabilities.Add(command[i+1]);
                    }
                }
                Console.WriteLine("**************************************************************************************");
                for (int i = 0; i < _infra.clients[0].capabilities.Count; i++)
                {
                    Console.WriteLine(_infra.clients[0].capabilities[i]);
                }
                
                return _parser.RespBulkString("OK");
        }

        return _parser.RespBulkString("OK");
    }

    public string Replication()
    {
        string role = $"role:{_config.role}";
        string masterReplid = $"master_replid:{_config.masterReplId}";
        string masterReplOffset = $"master_repl_offset:{_config.masterReplOffset}";

        string[] info = [role, masterReplid, masterReplOffset];

        string replicationData = string.Join("\r\n", info);
        
        Console.WriteLine(replicationData);
        return _parser.RespBulkString(replicationData);
    }
}