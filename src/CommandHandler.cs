
namespace codecrafters_redis;

public class CommandHandler
{
    private String _response;
    public CommandHandler(String[] command)
    {
        String cmd = command[0];
        switch(cmd){
            case "ping":
                _response = "+PONG\r\n";
                break;
            case "echo":
                _response = $"+{command[1]}\r\n";
                break;
            default:
                _response = "+No Response\r\n";
                break;
        }
    }
    public String GetResponse()
    {
        return _response;
    }
}