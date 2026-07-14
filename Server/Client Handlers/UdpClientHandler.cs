using Server.Interfaces;
using Server.IO_Handlers;
using Server.Servers;
using System.Net;
using System.Net.Sockets;
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
            string clientID = GetClientID(clientEP);
            _outputHandler.PrintMessageDetails(message, clientEP, clientID);

            if (message.Equals("options", StringComparison.OrdinalIgnoreCase))
                _outputHandler.DisplayOptions(clientEP);

            else if (message.Equals("join", StringComparison.OrdinalIgnoreCase))
                _outputHandler.DisplayGroupChats(clientEP);

            else if (message.Equals("chat", StringComparison.OrdinalIgnoreCase))
                _outputHandler.DisplayConnectedClients(clientEP);

            else if (message.StartsWith("join", StringComparison.OrdinalIgnoreCase))
                _inputHandler.JoinGroupChat(message, clientEP);

            else
                _inputHandler.HandleMessage(message, clientEP, clientID);
        }

        public string GetClientID(IPEndPoint clientEP)
        {               
            foreach (string username in UdpServer._all_clients.Keys)
            {
                if (UdpServer._all_clients[username].Equals(clientEP))
                    return username;
            }
            throw new Exception("No Client ID Found.");
        }

        public IPAddress GetClientIP(string user)
        {
            IPEndPoint ep = UdpServer._all_clients[user];
            return ep.Address;
        }

        public int GetClientPortNum(string user)
        {
            IPEndPoint ep = UdpServer._all_clients[user];
            return ep.Port;
        }

        public void AddClient(string username, IPEndPoint newClient)
        {
            if (UdpServer._all_clients.ContainsKey(username))
            {
                // attach last 4 numbers of the hash to make username almost always unique 
                string hash = username.GetHashCode().ToString();
                username += hash.Substring(hash.Length - 4);
            }
            UdpServer._all_clients.TryAdd(username, newClient);
            Console.WriteLine($"[SERVER] New User {username} logged in...\n");
        }        
    }
}
