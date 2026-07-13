using Server.InputHandlers;
using Server.Interfaces;
using Server.Servers;
using System.Net;
using System.Text;

namespace Server.Client_Handlers
{
    internal class UdpClientHandler : IClientHandler<IPEndPoint>
    {
        private readonly UdpIOHandler _handler = new UdpIOHandler();

        public void HandleNewClient(IPEndPoint clientEndPoint, byte[] recievedBytes)
        {
            string clientID = Encoding.UTF8.GetString(recievedBytes);
            AddClient(clientID, clientEndPoint);
            _handler.DisplayOptions(clientEndPoint);
        }

        public void HandleExistingClient(IPEndPoint clientEndPoint, string message)
        {
            string clientID = GetClientID(ref clientEndPoint);
            _handler.PrintMessageDetails(message, clientID);

            if (message.Equals("join", StringComparison.OrdinalIgnoreCase))
                _handler.DisplayGroupChats(clientEndPoint);

            else if (message.Equals("chat", StringComparison.OrdinalIgnoreCase))
                _handler.DisplayConnectedClients(clientEndPoint);

            else
                _handler.HandleMessage(message, clientEndPoint, clientID);
        }

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
