using System.Net;

namespace Server.Interfaces
{
    internal interface IClientHandler<T>
    {
        public IPAddress GetClientIP(string user);
        public int GetClientPortNum(string user);
        public void AddClient(string username, T clientEP);
    }
}
