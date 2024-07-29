namespace codecrafters_redis;

public class Store
{
    private Dictionary<String, Value> map;
    public string role="";
    public string MasterHost=".";
    public string MasterPort=".";
    public Store()
    {
        map = new Dictionary<String, Value>();
    }

    public Dictionary<String, Value> GetMap()
    {
        return this.map;
    }

/*    public void Set(String[] command, Store store, DateTime currTime)
    {
        if (command.Length == 3)
        {
            DateTime expiry = DateTime.MaxValue;
            Value val = new Value(command[2], currTime, expiry);
            store.GetMap()[command[1]] = val;
        }
        else if (command.Length == 5 && command[3].Equals("px"))
        {
            int delta = int.Parse(command[4]);

            DateTime expiry = currTime.AddMilliseconds(delta);
            Value val = new Value(command[2], currTime, expiry);
            store.GetMap()[command[1]] = val;
        }
        _response = "+OK\r\n";
    }

    public void Get(String[] command, Store store, DateTime currTime)
    {
        Value val = store.GetMap()[command[1]];

        Console.WriteLine("Value: " + val.val);
        Console.WriteLine("Expiry: " + val.expiry.ToString());
        if (currTime <= val.expiry)
        {
            _response = $"+{val.val}\r\n";
        }
        else
        {
            store.GetMap().Remove(command[1]);
            _response = $"$-1\r\n";
        }
    }*/
}

public class Value {
    public String val;
    public DateTime created;
    public DateTime expiry;
    public Value(String val, DateTime created, DateTime expiry)
    {
        this.val = val;
        this.created = created;
        this.expiry = expiry;
    }

}