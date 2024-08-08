namespace codecrafters_redis.src
{
    public class Infra
    {
        public List<Slave> clients = new List<Slave>();
        public List<Slave> clientsToFullSync = new List<Slave>();
    }

    public class Slave 
    {
        public int port;
        public string ipaddress;
        public List<string> capabilities;
        public Slave(int port,string ipaddress)
        {
            this.port = port;
            this.ipaddress = ipaddress;
            capabilities = new List<string>();
        }
    }
}
