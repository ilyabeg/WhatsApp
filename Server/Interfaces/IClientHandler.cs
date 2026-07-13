
using System.Net;
using System.Net.Sockets;

namespace Server.Interfaces
{
    internal interface IClientHandler<T>
    {
        public string GetClientID(ref T client);
        public IPAddress GetClientIP(string user);
        public int GetClientPortNum(string user);
        public void AddClient(string username, T client);
        public void SendToClient(T client, string msg);
    }
}
