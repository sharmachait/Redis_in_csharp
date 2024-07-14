namespace codecrafters_redis;

public class Store
{
    private Dictionary<String, String> map;

    public Store()
    {
        map = new Dictionary<string, string>();
    }

    public Dictionary<String, String> GetMap()
    {
        return this.map;
    }
}

public class Value {
    public String val;
    public DateTime created;
    public DateTime expiry;
    public Value(string val, DateTime created, DateTime expiry)
    {
        this.val = val;
        this.created = created;
        this.expiry = expiry;
    }

}