using System.Text;

namespace codecrafters_redis;

public class RespParser
{
    private String _data;
    public RespParser(byte[] command)
    {
        _data = Encoding.UTF8.GetString(command);
    }

    public String GetData()
    {
        return _data;
    }

    public String[] GetParts()
    {
        String[] parts = _data.Split("\r\n");
        Console.WriteLine("parts: ");
        int c = 0;
        foreach(String part in parts){
            Console.WriteLine(c+": "+part);
        }
        return parts;
    }
}


