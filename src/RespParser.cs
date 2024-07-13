using System.Text;

namespace codecrafters_redis;

public class RespParser
{
    private String _command;
    public RespParser(byte[] command)
    {
        _command = Encoding.UTF8.GetString(command);
    }

    public String GetCommand()
    {
        return _command;
    }
}