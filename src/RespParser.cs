using System.Text;

namespace codecrafters_redis;

public class RespParser
{
    public RespParser() {
        
    }

    public string[] Deserialize(byte[] command)
    {
        string _data = Encoding.UTF8.GetString(command);
        _data = _data.Substring(0, _data.IndexOf('\0'));
        string[] parts = _data.Split("\r\n");
        if (parts[0][0]=='*')
        {
            return ParseArray(parts);
        }
        return new string[] { "No","command"};
    }

    public string[] ParseArray(string[] parts)
    {
        string len = parts[0].Substring(1);
        int length = int.Parse(len);
        string[] _command = new string[length];
        _command[0] = parts[2].ToLower();
        int idx = 1;
        for (int i = 4; i < parts.Length; i+=2)
        {
            _command[idx++] = parts[i];
        }
        return _command;
    }

    public string RespBulkString(string response) {
        return "$" + response.Length + "\r\n" + response + "\r\n";
    }
    public string RespArray(string[] a) {
        var s= "*3\r\n$8\r\nREPLCONF\r\n$4\r\ncapa\r\n$6\r\npsync2\r\n";
        
        List<string> res = new List<string>();

        int len = a.Length;

        res.Add("*" + len);

        foreach (string e in a) {
            res.Add("$" + e.Length);
            res.Add(e);
        }

        return string.Join("\r\n",res)+"\r\n";
    }
    public string RespRdbFile(string content)
    {
        byte[] bytes = Convert.FromBase64String(content);
        string res = $"${bytes.Length}\r\n{content}";
        return res;
    }
}



