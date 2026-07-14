using Server.Handlers;
using Server.Helpers;
using Server.Interfaces;
using Server.Servers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.InputHandlers
{
    internal class TcpIOHandler
    {
        private TcpClientHandler _clientHandler = new TcpClientHandler();

        public void HandleMessage(string msg, TcpClient client, string sender)
        {
            string recieverID = "#", actualMsg = "#";
            try
            {
                if (msg == null || msg.IsWhiteSpace())
                    throw new Exception();

                recieverID = MessageProcessor.GetRecieverID(msg.Trim());
                actualMsg = MessageProcessor.GetActualMsg(msg.Trim());

                TcpClient reciever = TcpServer._all_clients[recieverID];
                _clientHandler.SendToClient(reciever, $"[{sender}]: {actualMsg}");
            }
            catch (Exception)
            {
                _clientHandler.SendToClient(client, "[SERVER] Invalid input.");
            }            
        }

        public void DisplayConnectedClients(TcpClient client)
        {
            StringBuilder output = new StringBuilder("[SERVER] Connected users:");

            foreach (string username in TcpServer._all_clients.Keys)
            {
                IPAddress userIP = _clientHandler.GetClientIP(username);
                int userPort = _clientHandler.GetClientPortNum(username);
                output.Append($"\n\t- {username} [{userIP} : {userPort}]");
            }
            output.Append("\n");

            _clientHandler.SendToClient(client, output.ToString());
        }

        public void PrintMessageDetails(string msg, string clientID)
        {
            if (msg == null || msg.IsWhiteSpace()) return;

            IPAddress clientIP = _clientHandler.GetClientIP(clientID);
            int clientPort = _clientHandler.GetClientPortNum(clientID);
            Console.WriteLine($"[SERVER] Recieved: '{msg}' from {clientID} [{clientIP} : {clientPort}].");
        }
    }
}
