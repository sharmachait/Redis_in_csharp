﻿using codecrafters_redis.src;
using System.Collections;
using System.Net;
using System.Text;

namespace codecrafters_redis;

public class CommandHandler
{
    private readonly RespParser _parser;
    private readonly Store _store;
    private readonly RedisConfig _config;
    private readonly Infra _infra;
    private int slaveId=0;

    public CommandHandler(Store store, RespParser parser, RedisConfig config, Infra infra)
    {
        _infra = infra;
        _parser = parser;
        _store = store;
        _config = config;
    }




    public async Task<string> Handle(string[] command, Client client) {

        string cmd = command[0];

        DateTime currTime = DateTime.Now;
        string res = "";

        switch (cmd)
        {
            case "ping":
                res = "+PONG\r\n";
                break;
                
            case "echo":
                res = $"+{command[1]}\r\n";
                break;
                
            case "get":
                res = _store.Get(command, currTime);
                break;

            case "set":
                res = Set(client.remoteIpEndPoint, command,currTime);
                _ = Task.Run(() => sendCommandToSlaves(_infra.slaves, command));
                break;

            case "info":
                res = Info(command);
                break;

            case "replconf":
                res = ReplConf(command, client);
                break;
            case "psync":
                res = await Psync(command, client);
                break;
            default:
                res = "+No Response\r\n";
                break;
        }
        if (res.Equals(""))
        {
            // dont write to client
        }
        // write to client res
        return res;
    }

    public void sendCommandToSlaves(List<Slave> slaves, string[] command)
    {
        foreach(Slave slave in slaves)
        {
            string commandRespString = _parser.RespArray(command);
            Console.WriteLine(commandRespString);
            slave.connection.Send(commandRespString);
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




    public string Set(IPEndPoint remoteIpEndPoint, string[] command, DateTime currTime)
    {
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
        var res = _store.Set(command, currTime);

        return res;
    }




    public string ReplConf(string[] command, Client client)
    {
        string clientIpAddress = client.remoteIpEndPoint.Address.ToString();
        int clientPort = client.remoteIpEndPoint.Port;

        switch (command[1])
        {
            case "listening-port":
                try
                {
                    Slave s = new Slave(++slaveId, client);
                    _infra.slaves.Add(s);

                    return _parser.RespBulkString("OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return _parser.RespBulkString("NOTOK");
                }
            case "capa":
                try
                {
                    int idx = _infra.slaves.FindIndex((x) => { return x.connection.ipAddress.Equals(clientIpAddress); });
                    for (int i = 0; i < command.Length; i++)
                    {
                        if (command[i].Equals("capa"))
                        {
                            _infra.slaves[idx].capabilities.Add(command[i + 1]);
                        }
                    }

                    return _parser.RespBulkString("OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return _parser.RespBulkString("NOTOK");
                }

        }
        return _parser.RespBulkString("OK");
    }



    public async Task<string> Psync(string[] command, Client client)
    {
        try
        {
            string clientIpAddress = client.remoteIpEndPoint.Address.ToString();
            int clientPort = client.remoteIpEndPoint.Port;

            string replicationIdMaster = command[1];
            string replicationOffsetMaster = command[2];

            if (replicationIdMaster.Equals("?") && replicationOffsetMaster.Equals("-1"))
            {
                string emptyRdbFileBase64 =
           "UkVESVMwMDEx+glyZWRpcy12ZXIFNy4yLjD6CnJlZGlzLWJpdHPAQPoFY3RpbWXCbQi8ZfoIdXNlZC1tZW3CsMQQAPoIYW9mLWJhc2XAAP/wbjv+wP9aog==";
                
                byte[] rdbFile = Convert.FromBase64String(emptyRdbFileBase64);
                
                byte[] rdbResynchronizationFileMsg =
                    Encoding.ASCII.GetBytes($"${rdbFile.Length}\r\n")
                        .Concat(rdbFile)
                        .ToArray();

                client.Send(
                    $"+FULLRESYNC {_config.masterReplId} {_config.masterReplOffset}\r\n", 
                    rdbResynchronizationFileMsg
                );

                return "";
            }
            else
            {
                return "Options not supported";
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            return "Options not supported";
        }
    }
}