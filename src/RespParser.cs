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
        Console.WriteLine("****************************************************************************************************");
        Console.WriteLine("split on *");

        string[] commands = _data.Split('*');
        int i = 0;
        foreach(string c in commands)
        {
            Console.WriteLine("command id"+i++);
            Console.WriteLine(c);
        }


        string[] parts = _data.Split("\r\n");



        if (parts[0][0]=='*')
        {
            string[] res  = ParseArray(parts);
            
            return res;
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
        Console.WriteLine("****************************************************************************************************");
        Console.WriteLine("parts with out split");

        foreach (string c in _command)
        {
            Console.WriteLine(c);
        }
        return _command;
    }

    public string RespBulkString(string response) {
        return "$" + response.Length + "\r\n" + response + "\r\n";
    }
    public string RespArray(string[] a) {        
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



