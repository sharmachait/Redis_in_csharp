namespace codecrafters_redis;

public class CommandHandler
{
    private String _response;
    public CommandHandler(String[] command, Store store)
    {
        String cmd = command[0];

        switch(cmd){
            case "ping":
                _response = "+PONG\r\n";
                break;
            case "echo":
                _response = $"+{command[1]}\r\n";
                break;
            case "get":
                try
                {
                    _response = $"+{store.GetMap()[command[1]]}\r\n";
                }
                catch (KeyNotFoundException)
                {
                    _response = "error getting value";
                }
                break;
            case "set":
                store.GetMap()[command[1]] = command[2];
                _response = "+OK\r\n";
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