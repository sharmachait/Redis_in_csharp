using codecrafters_redis.src;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Text.Json;
namespace codecrafters_redis;

class Program
{
    ConcurrentBag<Socket> replicas = [];
    int myPort = 6379;
    string myRole = "master";
    string isReplicaOf = "None";
    string replicaId = "8371b4fb1155b71f4a04d3e1bc3e18c4a990aeeb";
    int replicaOffset = 0;
    Dictionary<string, string> kv = new Dictionary<string, string>();
    Dictionary<string, long> ttl = new Dictionary<string, long>();
    async Task Main(string[] args)
    {

        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--port":
                    myPort = int.Parse(args[i + 1]);
                    break;
                case "--replicaof":
                    myRole = "slave";
                    isReplicaOf = args[i + 1];
                    break;
                default:
                    break;
            }
        }
        // Create a dictionary of type <string,string> to store the key-value pairs
        

        TcpListener server = new TcpListener(IPAddress.Any, myPort);

        if (myRole == "slave")
        {
            _ = Task.Run(async () => { await StartReplica(); });
        }

        try
        {
            server.Start();
            while (true)
            {
                Console.WriteLine("Waiting for connection...");
                var client = await server.AcceptSocketAsync();
                // HandleRequestAsync(client);
                _ = HandleRequestAsync(client);
            }
        }
        finally
        {
            server.Stop();
        }
    }

    async Task StartReplica()
    {
        Console.WriteLine($"Replicating from {isReplicaOf}");
        // Connect to the master server
        var hostParts = isReplicaOf.Split(" ");
        var master = new TcpClient();
        master.Connect(hostParts[0], int.Parse(hostParts[1]));
        var masterClient = master.Client;
        await HandleHandshakeAsync(masterClient);//////////////////////////////////////////////////

        _ = HandleRequestAsync(masterClient, IsMaster: true);
    }

    async Task HandleHandshakeAsync(Socket client)
    {
        var handshakeParts = GetHandshakeParts();
        var buffer = new byte[1024];
        foreach (var part in handshakeParts)
        {
            Console.WriteLine($"Sending: {part}");
            byte[] data = Encoding.ASCII.GetBytes(part);
            await client.SendAsync(data);
            await client.ReceiveAsync(buffer);
            var response = Encoding.ASCII.GetString(buffer);
            Console.WriteLine($"Response: {response}");
            // if response starts with +FULLRESYNC, await the redis file
            // if (response.ToLower().Contains("+fullresync"))
            // {
            //   Console.WriteLine("Receiving full resync");
            //   _ = Task.Run(async () => {
            //     var buffer = new byte[1024];
            //     await client.ReceiveAsync(buffer);
            //     response = Encoding.ASCII.GetString(buffer);
            //     Console.WriteLine($"Response: {response}");
            //   });
            // }
        }
    }

    string[] GetHandshakeParts()
    {
        var lenListeningPort = myPort.ToString().Length;
        var listeningPort = myPort.ToString();
        var mystrings =
            new string[] { "*1\r\n$4\r\nPING\r\n",
                     "*3\r\n$8\r\nREPLCONF\r\n$14\r\nlistening-port\r\n$" +
                         lenListeningPort.ToString() + "\r\n" + listeningPort +
                         "\r\n",
                     "*3\r\n$8\r\nREPLCONF\r\n$4\r\ncapa\r\n$6\r\npsync2\r\n",
                     "*3\r\n$5\r\nPSYNC\r\n$1\r\n?\r\n$2\r\n-1\r\n" };
        return mystrings;
    }

    async Task HandleRequestAsync(Socket client, bool IsMaster = false)
    {
        // client.SetSocketOption(SocketOptionLevel.Socket,
        // SocketOptionName.KeepAlive, true);
        client.ReceiveTimeout = 5000;
        client.SendTimeout = 5000;
        long lastRead = GetEpochNow();
        while (client.Connected)
        {
            // long lastReadMs = GetEpochNow() - lastRead;
            // if(lastReadMs > 5000)
            // {
            //   Console.WriteLine($"Closing connection to {client.RemoteEndPoint}
            //   lastReadMs: {lastReadMs}"); client.Close(); break;
            // }
            byte[] buffer = new byte[1024];
            int bytesRead = await client.ReceiveAsync(buffer);
            if (bytesRead > 0)
            {
                lastRead = GetEpochNow();
                Console.WriteLine($"BytesRead from {client.RemoteEndPoint} {bytesRead}");

                var result = await HandleCommandsResponse(client, buffer, bytesRead, IsMaster);
                Console.WriteLine($"Result: {result}");
            }
        }
    }


    void HandleReplicas(string[] request)
    {
        if (myRole == "master" && replicas.Count > 0)
        {
            foreach (var replica in replicas)
            {
                var socket = replica;
                if (socket.Connected)
                {
                    _ = Task.Run(async () =>
                    {
                        await replica.SendAsync(
                            Encoding.ASCII.GetBytes(HandleRespArray(request)));
                    });
                    Console.WriteLine($"Sent data to replica: {replica.RemoteEndPoint}");
                }
                else
                {
                    replicas.TryTake(out socket);
                    Console.WriteLine(
                        $"Replica not connected, removed from list: {socket.RemoteEndPoint}");
                }
            }
        }
        else
        {
            Console.WriteLine(
                $"Not a master or no replicas available role: {myRole} replicas: {replicas.Count}");
        }
    }

    string HandleRespArray(string[] request)
    {
        int lengthRequestArray = (request.Length - 1) / 2;
        var outstr = "";
        outstr += $"*{lengthRequestArray}\r\n";
        for (int i = 2; i < request.Length; i += 2)
        {
            outstr += $"${request[i].Length}\r\n{request[i]}\r\n";
        }
        return outstr;
    }


    async Task<bool> HandleCommandsResponse(Socket client, byte[] byteData,
                                            int bytesRead, bool IsMaster = false)
    {
        var data = Encoding.ASCII.GetString(byteData, 0, bytesRead);
        Console.WriteLine($"Data: {data}");
        var commands = data.Split("*");
        // Process each command
        foreach (var command in commands)
        {
            var cmd = "*" + command;
            Console.WriteLine("Command: " + command);
            var commandParts = cmd.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            if (commandParts.Length > 0)
            {
                var result =
                    await HandleCommandResponse(commandParts, client, byteData, IsMaster);
                Console.WriteLine($"Result: {result}");
            }
        }
        return true;
    }

    async Task<bool> HandleCommandResponse(string[] request, Socket client, byte[] byteData, bool IsMaster = false)
    {
        string reply = "-ERR unknown command\r\n";
        string json = JsonSerializer.Serialize(request);
        Console.WriteLine($"Received command: {json}");
        if (request.Length < 3)
        {
            // await client.SendAsync(Encoding.ASCII.GetBytes(reply));
            return false;
        }
        switch (request[2].ToLower())
        {
            case "ping":
                reply = "+PONG\r\n";
                break;
            case "echo":
                reply = $"${request[4].Length}\r\n{request[4]}\r\n";
                break;
            case "set":
                reply = HandleSet(request);
                HandleReplicas(request);
                if (!IsMaster)
                    await client.SendAsync(Encoding.ASCII.GetBytes(reply));
                return true;
            case "get":
                reply = HandleGet(request);
                break;
            case "info":
                reply = HandleInfo(request);
                break;
            case "replconf":
                reply = "+OK\r\n";
                break;
            case "psync":
                await client.SendAsync(Encoding.ASCII.GetBytes(
                    "+FULLRESYNC " + replicaId + " " + replicaOffset + "\r\n"));
                string emptyRdbFileBase64 =
                    "UkVESVMwMDEx+glyZWRpcy12ZXIFNy4yLjD6CnJlZGlzLWJpdHPAQPoFY3RpbWXCbQi8ZfoIdXNlZC1tZW3CsMQQAPoIYW9mLWJhc2XAAP/wbjv+wP9aog==";
                byte[] rdbFile = System.Convert.FromBase64String(emptyRdbFileBase64);
                byte[] rdbResynchronizationFileMsg =
                    Encoding.ASCII.GetBytes($"${rdbFile.Length}\r\n")
                        .Concat(rdbFile)
                        .ToArray();
                await client.SendAsync(rdbResynchronizationFileMsg);
                if (myRole == "master")
                {
                    replicas.Add(client);
                    Console.WriteLine($"Added replica: {client.RemoteEndPoint}");
                }
                return true;
            default:
                break;
        }

        if (!IsMaster)
            await client.SendAsync(Encoding.ASCII.GetBytes(reply));
        return true;
    }
    string EncodeString(string str) { return $"${str.Length}\r\n{str}\r\n"; }
    string HandleInfo(string[] request)
    {
        var values = new Dictionary<string, string> { { "role", myRole },
                                                { "replicaof", isReplicaOf },
                                                { "master_replid", replicaId },
                                                { "master_repl_offset",
                                                  replicaOffset.ToString() } };
        var msgString = string.Join("\n", values.Select(x => $"{x.Key}:{x.Value}"));
        if (request.Length > 4 && request[4] == "replication")
        {
            values = new Dictionary<string, string> { { "master_replid", replicaId },
                                              { "master_repl_offset",
                                                replicaOffset.ToString() } };
            return EncodeString(msgString);
        }
        return EncodeString(msgString);
    }
    string HandleGet(string[] request)
    {
        string reply = "-ERR unknown command\r\n";
        if (kv.ContainsKey(request[4]))
        {
            // Check if the key is expired
            if (ttl.ContainsKey(request[4]))
            {
                if (ttl[request[4]] < GetEpochNow())
                {
                    // Key is expired, remove it from kv and ttl
                    kv.Remove(request[4]);
                    ttl.Remove(request[4]);
                    reply = "$-1\r\n";
                }
                else
                {
                    // Key is not expired, return the value
                    var value = kv[request[4]];
                    reply = $"${value.Length}\r\n{value}\r\n";
                }
            }
            else
            {
                // TTL value doesn't exist for the key, return the value
                var value = kv[request[4]];
                reply = $"${value.Length}\r\n{value}\r\n";
            }
        }
        else
        {
            reply = "$-1\r\n";
        }
        return reply;
    }
    long GetEpochNow()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
    }
    string HandleSet(string[] request)
    {
        if (request.Length >= 10 && request[8].ToLower() == "px")
        {
            // request[10] is the TTL in ms, add to ttl and add to kv
            long ttlValue;
            if (long.TryParse(request[10], out ttlValue))
            {
                ttl[request[4]] = ttlValue + GetEpochNow();
            }
            else
            {
                return "-ERR invalid expire time in set\r\n";
            }
        }
        kv[request[4]] = request[6];
        return "+OK\r\n";
    }
    //static async Task Main(string[] args)
    //{
    //    RedisConfig config = new RedisConfig();

    //    for (int i = 0; i < args.Length; i++)
    //    {
    //        switch (args[i])
    //        {
    //            case "--port":
    //                config.port = int.Parse(args[i + 1]);
    //                break;
    //            case "--replicaof":
    //                config.role = "slave";
    //                Console.WriteLine(args[i + 1]);
    //                string masterHost = args[i + 1].Split(' ')[0];
    //                int masterPort = int.Parse(args[i + 1].Split(' ')[1]);
    //                config.masterHost = masterHost;
    //                config.masterPort = masterPort;
    //                break;
    //            default:
    //                break;
    //        }
    //    }


    //    //int portFlag = args.ToList().IndexOf("--port");
    //    //int replicaFlag = args.ToList().IndexOf("--replicaof");

    //    ////RedisConfig config;

    //    //if (portFlag > -1)
    //    //{
    //    //    int port = int.Parse(args[portFlag + 1]);

    //    //    if (replicaFlag > -1)
    //    //    {
    //    //        string masterHost = args[replicaFlag + 1].Split(' ')[0];

    //    //        int masterPort = int.Parse(args[replicaFlag + 1].Split(' ')[1]);

    //    //        config = new RedisConfig("slave", port, masterPort, masterHost);
    //    //    }
    //    //    else
    //    //    {
    //    //        config = new RedisConfig(port);
    //    //    }
    //    //}
    //    //else
    //    //{
    //    //    config = new RedisConfig();
    //    //}


    //    var serviceProvider = new ServiceCollection()
    //        .AddSingleton(config)
    //        .AddSingleton<Store>()
    //        .AddSingleton<Infra>()
    //        .AddSingleton<RespParser>()
    //        .AddSingleton<CommandHandler>()
    //        .AddSingleton<TcpServer>()
    //        .BuildServiceProvider();

    //    TcpServer app = serviceProvider.GetRequiredService<TcpServer>();


    //    var startTask = Task.Run(() => app.StartAsync());

    //    // Conditionally start the replica on a different thread if required
    //    if (config.role == "slave")
    //    {
    //        var startReplicaTask = Task.Run(() => app.StartReplicaAsync());
    //        await Task.WhenAll(startTask, startReplicaTask);
    //    }
    //    else
    //    {
    //        //Console.WriteLine($"Server starting at {config.port}*********************************** server.cs");
    //        await startTask;
    //    }
    //}

}

