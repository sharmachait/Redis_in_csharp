namespace codecrafters_redis;

public class CommandHandler
{
    private string _response;
    private RespParser _parser;
    Store _store;
    public CommandHandler(string[] command, Store store, RespParser parser)
    {
        _parser = parser;
        _store = store;
        string cmd = command[0];
        DateTime currTime = DateTime.Now;
        switch (cmd){
            case "ping":
                _response = "+PONG\r\n";
                break;
            case "echo":
                _response = $"+{command[1]}\r\n";
                break;
            case "get":
                try
                {
                    Get(command, currTime);
                }
                catch (KeyNotFoundException)
                {
                    _response = $"$-1\r\n";
                }
                break;
            case "set":
                try
                {
                    Set(command, currTime);
                }
                catch (KeyNotFoundException)
                {
                    _response = $"$-1\r\n";
                }
                break;
            case "info":
                Info(command);
                break;
            default:
                _response = "+No Response\r\n";
                break;
        }
    }
    
    public string GetResponse()
    {
        return _response;
    }


    public void Set(string[] command, DateTime currTime)
    {
        if (command.Length == 3)
        {
            DateTime expiry = DateTime.MaxValue;
            Value val = new Value(command[2], currTime, expiry);
            _store.GetMap()[command[1]] = val;
        }
        else if (command.Length == 5 && command[3].Equals("px"))
        {
            int delta = int.Parse(command[4]);

            DateTime expiry = currTime.AddMilliseconds(delta);
            Value val = new Value(command[2], currTime, expiry);
            _store.GetMap()[command[1]] = val;
        }
        _response = "+OK\r\n";
    }

    public void Get(string[] command,DateTime currTime)
    {
        Value val = _store.GetMap()[command[1]];

        Console.WriteLine("Value: " + val.val);
        Console.WriteLine("Expiry: " + val.expiry.ToString());
        if (currTime <= val.expiry)
        {
            _response = $"+{val.val}\r\n";
        }
        else
        {
            _store.GetMap().Remove(command[1]);
            _response = $"$-1\r\n";
        }
    }


    public void Info(string[] command)
    {
        switch (command[1])
        {
            case "replication":
                try
                {
                    Replication();
                }
                catch (Exception e)
                {
                    _response = e.Message;
                }
                break;
            default:
                _response = "Invalid options";
                break;
        }
    }
    public void Replication()
    {
        string replication = "role:"+_store.role;
        _response = _parser.MakeBulkString(replication);
        Console.WriteLine(_response);
    }
}