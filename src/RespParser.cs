using System.Text;

namespace codecrafters_redis;

public class RespParser
{
    private String _data;
    private String[] _command;
    public RespParser(byte[] command)
    {
        _data = Encoding.UTF8.GetString(command);
        MakeCommand();
    }
    public String[] GetCommand()
    {
        return _command;
    }

    public void MakeCommand()
    {
        String[] parts = _data.Split("\r\n");
        if (parts[0][0]=='*')
        {
            ParseArray(parts);
        }
    }

    public void ParseArray(String[] parts)
    {
        Console.WriteLine("Parsing...."+parts[0].Substring(1));
        String len = parts[0].Substring(1);
        Console.WriteLine("len: "+len);
        int length = int.Parse(len);
        Console.WriteLine("length: "+length);

        _command = new String[length];
        _command[0] = parts[2].ToLower();
        int idx = 1;
        for (int i = 4; i < parts.Length; i+=2)
        {
            _command[idx++] = parts[i];
        }
    }
}


