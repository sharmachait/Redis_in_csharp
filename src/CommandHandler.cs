namespace codecrafters_redis;

public class CommandHandler
{
    private readonly RespParser _parser;
    private readonly Store _store;
    private readonly RedisConfig _config;

    public CommandHandler(Store store, RespParser parser, RedisConfig config)
    {
        _parser = parser;
        _store = store;
        _config = config;
    }

    public string Handle(string[] command) {
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
                return _store.Set(command, currTime);
                
            case "info":
                return Info(command);
                
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
    public string Replication()
    {
        string role = $"role:{_config.role}";
        string masterReplid = $"master_replid:{_config.masterReplId}";
        string masterReplOffset = $"master_repl_offset:{_config.masterReplOffset}";

        string[] info = [role, masterReplid, masterReplOffset];

        string replicationData = string.Join("\r\n", info);
        
            //"role:" + _config.role+"\r\n" +
            //"master_replid:" + _config.masterReplId+"\r\n" +
            //"master_repl_offset:" + _config.masterReplOffset;


        Console.WriteLine(replicationData);
        return _parser.MakeBulkString(replicationData);
    }
}