using System.Text;

namespace codecrafters_redis;

public class RespParser
{
    private string _data;
    private string[] _command;
    public RespParser() {
        
    }
    public RespParser(byte[] command)
    {
        _data = Encoding.UTF8.GetString(command);
        MakeCommand();
    }
    public string[] GetCommand()
    {
        return _command;
    }

    public void MakeCommand()
    {
        string[] parts = _data.Split("\r\n");
        if (parts[0][0]=='*')
        {
            ParseArray(parts);
        }
    }

    public void ParseArray(string[] parts)
    {
        string len = parts[0].Substring(1);
        int length = int.Parse(len);
        _command = new string[length];
        _command[0] = parts[2].ToLower();
        int idx = 1;
        for (int i = 4; i < parts.Length; i+=2)
        {
            _command[idx++] = parts[i];
        }
    }
    public string MakeBulkString(string response) {
        return @"$" + response.Length + "\r\n" + response + "\r\n";
    }
}



