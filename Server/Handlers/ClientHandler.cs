using Server.Servers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Handlers
{
    internal class ClientHandler
    {
        public static string GetClientID(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];

            int totalRead = stream.Read(buffer, 0, buffer.Length);
            string username = Encoding.UTF8.GetString(buffer, 0, totalRead);           
            return username;
        }

        public static IPAddress GetClientIP(string user)
        {
            IPEndPoint endPoint = TcpServer._all_clients[user].Client.RemoteEndPoint as IPEndPoint;
            return endPoint.Address;
        }

        public static int GetClientPortNum(string user)
        {
            IPEndPoint endPoint = TcpServer._all_clients[user].Client.RemoteEndPoint as IPEndPoint;
            return endPoint.Port;
        }

        public static void AddClient(string username, TcpClient client)
        {
            if (TcpServer._all_clients.ContainsKey(username))
            {
                // attach last 4 numbers of the hash to make username almost always unique 
                string hash = username.GetHashCode().ToString();
                username += hash.Substring(hash.Length - 4);
            }
            TcpServer._all_clients.TryAdd(username, client);
            Console.WriteLine($"\n[SERVER] New User {username} logged in...\n");
        }

        public static void SendToClient(TcpClient client, string msg)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}
