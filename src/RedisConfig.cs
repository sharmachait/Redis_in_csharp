using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis;
public class RedisConfig
{
    public string role; 
    public int port;
    public int masterPort;
    public string masterHost;

    public RedisConfig(string role, int port, int masterPort, string masterHost)
    {
        this.role = role;
        this.port = port;
        this.masterHost = masterHost;
        this.masterPort = masterPort;
    }

    public RedisConfig(int port)
    {
        this.role = "master";
        this.port = port;
        this.masterHost = ".";
        this.masterPort = int.MinValue;
    }

    public RedisConfig()
    {
        this.role = "master";
        this.port = 6379;
        this.masterHost = ".";
        this.masterPort = int.MinValue;
    }

}
