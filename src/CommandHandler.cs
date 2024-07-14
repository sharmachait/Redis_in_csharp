﻿namespace codecrafters_redis;

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
                DateTime currTime = DateTime.Now;
                try
                {
                    Value val = store.GetMap()[command[1]];
                    if(currTime >= val.expiry)
                    {
                        _response = $"+{val.val}\r\n";
                    }
                    else
                    {
                        store.GetMap().Remove(command[1]);
                        _response = $"$-1\r\n";
                    }
                }
                catch (KeyNotFoundException)
                {
                    
                    _response = "error getting value";
                }
                break;
            case "set":
                DateTime currTime = DateTime.Now;
                if (command.Length == 3)
                {
                    
                    DateTime expiry = DateTime.MaxValue;
                    Value val = new Value(command[2], currTime, expiry);
                    store.GetMap()[command[1]] = val;
                }
                else if (command.Length == 5 && command[3].Equals("pc"))
                {
                    int delta = int.Parse(command[4]);
                    
                    DateTime expiry = currTime.AddMilliseconds(delta);
                    Value val = new Value(command[2], currTime, expiry);
                    store.GetMap()[command[1]] = val;
                } 
                
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