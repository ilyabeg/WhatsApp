using Server.Interfaces;
using Server.IO_Handlers;
using Server.Servers;
using System.Net;
using System.Text;

namespace Server.Client_Handlers
{
    internal class UdpClientHandler : IClientHandler<IPEndPoint>
    {
        UdpOutputHandler _outputHandler;
        UdpInputHandler _inputHandler;
        public UdpClientHandler(UdpOutputHandler o, UdpInputHandler i)
        {
            _outputHandler = o;
            _inputHandler = i;
        }

        public void HandleNewClient(IPEndPoint clientEP, byte[] recievedBytes)
        {
            string clientID = Encoding.UTF8.GetString(recievedBytes);
            AddClient(clientID, clientEP);
            _outputHandler.DisplayOptions(clientEP);
        }

        public void HandleExistingClient(IPEndPoint clientEP, string message)
        {
            string clientID = GetClientID(ref clientEP);
            _outputHandler.PrintMessageDetails(message, clientEP, clientID);

            if (message.Equals("join", StringComparison.OrdinalIgnoreCase))
                _outputHandler.DisplayGroupChats(clientEP);

            else if (message.Equals("chat", StringComparison.OrdinalIgnoreCase))
                _outputHandler.DisplayConnectedClients(clientEP);

            else if (message.StartsWith("join", StringComparison.OrdinalIgnoreCase))
                _inputHandler.JoinGroupChat(message, clientEP);

            else
                _inputHandler.HandleMessage(message, clientEP, clientID);
        }

        public string GetClientID(ref IPEndPoint clientEP)
        {               
            foreach (string username in UdpServer._all_clients.Keys)
            {
                if (UdpServer._all_clients[username].Equals(clientEP))
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

        public void AddClient(string username, IPEndPoint clientEP)
        {
            if (UdpServer._all_clients.ContainsKey(username))
            {
                // attach last 4 numbers of the hash to make username almost always unique 
                string hash = username.GetHashCode().ToString();
                username += hash.Substring(hash.Length - 4);
            }
            UdpServer._all_clients.TryAdd(username, clientEP);
            Console.WriteLine($"[SERVER] New User {username} logged in...\n");
        }        
    }
}
