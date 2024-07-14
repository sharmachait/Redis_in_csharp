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