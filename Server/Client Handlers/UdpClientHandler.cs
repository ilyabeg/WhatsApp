using Server.Interfaces;
using Server.Servers;
using System.Net;
using System.Text;

namespace Server.Client_Handlers
{
    internal class UdpClientHandler : IClientHandler<IPEndPoint>
    {
        public string GetClientID(ref IPEndPoint clientEndPoint)
        {               
            foreach (string username in UdpServer._all_clients.Keys)
            {
                if (UdpServer._all_clients[username].Equals(clientEndPoint))
                    return username;
            }
            return null;
        }

        public IPAddress GetClientIP(string user)
        {
            return UdpServer._all_clients[user].Address;
        }

        public int GetClientPortNum(string user)
        {
            return UdpServer._all_clients[user].Port;
        }

        public void AddClient(string username, IPEndPoint client)
        {
            if (UdpServer._all_clients.ContainsKey(username))
            {
                // attach last 4 numbers of the hash to make username almost always unique 
                string hash = username.GetHashCode().ToString();
                username += hash.Substring(hash.Length - 4);
            }
            UdpServer._all_clients.TryAdd(username, client);
            Console.WriteLine($"[SERVER] New User {username} logged in...\n");
        }

        public void SendToClient(IPEndPoint clientEndPoint, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            UdpServer._listener.Send(buffer, buffer.Length, clientEndPoint);
        }
    }
}
