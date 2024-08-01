using System;
namespace codecrafters_redis;
public class Config
{
	public string role;
	public int port;
	public int masterPort;
	public string masterHost;

    public Config(string role, int port, int masterPort, string masterHost)
	{
		this.role = role;
		this.port = port;
		this.masterHost = masterHost;
		this.masterPort = masterPort;
	}

    public Config(int port)
    {
        this.role = "master";
        this.port = port;
        this.masterHost = ".";
		this.masterPort = int.MinValue;
    }

    public Config()
    {
        this.role = "slave";
        this.port = port;
        this.masterHost = "NA";
        this.masterPort = 6379;
    }

}
