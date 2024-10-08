﻿namespace codecrafters_redis;

using codecrafters_redis.src;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TcpServer
{
    public readonly TcpListener _server;
    private readonly RespParser _parser;
    private readonly CommandHandler _handler;
    private readonly RedisConfig _config;
    private readonly Infra _infra;
    private int id;


    public TcpServer(RedisConfig config, Store store, RespParser parser, CommandHandler handler, Infra infra)
    {
        _handler = handler;

        _parser = parser;

        _config = config;

        _infra = infra;

        id = 0;

        _server = new TcpListener(IPAddress.Any, config.port);
    }


    public async Task StartAsync()
    {
        Console.WriteLine("started ************************************************************* tcpserver.cs");
        _server.Start();
        
        Console.WriteLine($"Server started at {_config.port}");

        while (true)
        {
            TcpClient socket = await _server.AcceptTcpClientAsync();
            id++;
            IPEndPoint? remoteIpEndPoint = socket.Client.RemoteEndPoint as IPEndPoint;
            if (remoteIpEndPoint == null)
                return;

            NetworkStream stream = socket.GetStream();

            Client client = new Client(socket, remoteIpEndPoint, stream, id);

            _infra.clients.Add(client);

            _ = Task.Run(async () => await HandleClientAsync(client));
        }
    }
    public async Task HandleClientAsync(Client client)
    {

        while (client.socket.Connected)
        {
            if (client.stream.DataAvailable)
            {
                byte[] buffer = new byte[client.socket.ReceiveBufferSize];
                int bytesRead = await client.stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    List<string[]> commands = _parser.Deserialize(buffer);

                    foreach (string[] command in commands)
                    {
                        string response = await _handler.Handle(command, client);
                        client.Send(response);
                    }
                }
            }
        }
    }

    public async Task StartReplicaAsync()
    {
        TcpClient master = new TcpClient();

        Console.WriteLine($"Server started at {_config.port}");
        Console.WriteLine($"Replicating from {_config.masterHost}: {_config.masterPort}");
        master.Connect(_config.masterHost, _config.masterPort);
        await InitiateSlaveryAsync(master);
        _ = Task.Run(async () => await StartMasterPropagation(master));
        
    }
    //done by slave instace
    //dont need to create the slave object here
    public async Task InitiateSlaveryAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream, Encoding.UTF8);

        string[] pingCommand = ["PING"];
        Console.WriteLine($"Sending: {_parser.RespArray(pingCommand)}");
        await stream.WriteAsync(Encoding.UTF8.GetBytes(_parser.RespArray(pingCommand)));
        string response =await reader.ReadLineAsync();
        if (!"+PONG".Equals(response))
            return ;
        Console.WriteLine($"Response: {response}");

        string[] ReplconfPortCommand = ["REPLCONF", "listening-port", _config.port.ToString()];
        Console.WriteLine($"Sending: {_parser.RespArray(ReplconfPortCommand)}");
        await stream.WriteAsync(Encoding.UTF8.GetBytes(_parser.RespArray(ReplconfPortCommand)));
        response = await reader.ReadLineAsync();
        if (!"+OK".Equals(response))
            return ;
        Console.WriteLine($"Response: {response}");

        string[] ReplconfCapaCommand = ["REPLCONF", "capa", "psync2"];
        Console.WriteLine($"Sending: {_parser.RespArray(ReplconfCapaCommand)}");
        await stream.WriteAsync(Encoding.UTF8.GetBytes(_parser.RespArray(ReplconfCapaCommand)));
        response = await reader.ReadLineAsync();
        if (!"+OK".Equals(response))
            return ;
        Console.WriteLine($"Response: {response}");

        Console.WriteLine("ready to process commands from master");

        string[] PsyncCommand = ["PSYNC", "?", "-1"];
        Console.WriteLine($"Sending: {_parser.RespArray(PsyncCommand)}");
        await stream.WriteAsync(Encoding.UTF8.GetBytes(_parser.RespArray(PsyncCommand)));
        //response = await reader.ReadLineAsync();
        //Console.WriteLine($"Response: {response}");

        //if (response == null || !"+FULLRESYNC".Equals(response.Substring(0, response.IndexOf(" "))))
        //    return null;

        //do multi thread to listen from master

    }

    public async Task StartMasterPropagation(TcpClient ConnectionWithMaster)
    {
        NetworkStream stream = ConnectionWithMaster.GetStream();
        while (ConnectionWithMaster.Connected)
        {
            if (stream.DataAvailable)
            {
                byte[] buffer = new byte[ConnectionWithMaster.ReceiveBufferSize];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    List<string[]> commands = _parser.Deserialize(buffer.Take(bytesRead).ToArray());

                    foreach (string[] command in commands)
                    {

                        string response = await _handler.HandleMasterCommands(command);
                    }
                }
            }
        }
    }

}


