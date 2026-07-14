using Server.Interfaces;
using Server.Servers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Handlers
{
    internal class TcpClientHandler : IClientHandler<TcpClient>
    {
        private readonly int _bufferSize = 4096;

        public string GetClientID(TcpClient client)
        {
            byte[] buffer = new byte[_bufferSize];
            NetworkStream stream = client.GetStream();

            int totalRead = stream.Read(buffer, 0, buffer.Length);
            string username = Encoding.UTF8.GetString(buffer, 0, totalRead);           
            return username;
        }

        public IPAddress GetClientIP(string user)
        {
            IPEndPoint endPoint = TcpServer._all_clients[user].Client.RemoteEndPoint as IPEndPoint;
            return endPoint.Address;
        }

        public int GetClientPortNum(string user)
        {
            IPEndPoint endPoint = TcpServer._all_clients[user].Client.RemoteEndPoint as IPEndPoint;
            return endPoint.Port;
        }

        public void AddClient(string username, TcpClient client)
        {
            if (TcpServer._all_clients.ContainsKey(username))
            {
                // attach last 4 numbers of the hash to make username almost always unique 
                string hash = username.GetHashCode().ToString();
                username += hash.Substring(hash.Length - 4);
            }
            TcpServer._all_clients.TryAdd(username, client);
            Console.WriteLine($"[SERVER] New User {username} logged in...\n");
        }

        public void SendToClient(TcpClient client, string msg)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}
