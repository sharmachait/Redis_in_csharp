using System.Text;

namespace codecrafters_redis;

public class RespParser
{
    private String _data;
    private String[] _command;
    public RespParser(byte[] command)
    {
        _data = Encoding.UTF8.GetString(command);
    }

    public String GetData()
    {
        return _data;
    }

    public String[] GetCommand()
    {
        return _command;
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

    public void MakeCommand()
    {
        String[] parts = GetParts();
        if (parts[0][0] == '*')
        {
            ParseArray(parts);
        }
    }

    public void ParseArray(String[] parts)
    {
        String len = parts[0].Substring(0, parts[0].Length);
        int length = int.Parse(len);
        _command = new String[length];
        _command[0] = parts[2].ToLower();
        int idx = 1;
        for (int i = 4; i < parts.Length; i+=2)
        {
            _command[idx++] = parts[i];
        }
    }
}


