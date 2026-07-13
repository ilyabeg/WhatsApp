using Server.Client_Handlers;
using Server.Helpers;
using Server.Interfaces;
using Server.Objects;
using Server.Servers;
using System.Net;
using System.Text;

namespace Server.InputHandlers
{
    internal class UdpIOHandler : IIOHandler<IPEndPoint>
    {
        private UdpClientHandler _clientHandler = new UdpClientHandler();

        public void HandleMessage(string msg, IPEndPoint client, string sender)
        {
            string recieverID = "#", actualMsg = "#";
            try
            {
                if (msg == null || msg.IsWhiteSpace())
                    throw new Exception();

                recieverID = MessageProcessor.GetRecieverID(msg);
                actualMsg = MessageProcessor.GetActualMsg(msg);

                IPEndPoint reciever = UdpServer._all_clients[recieverID];

                byte[] message = Encoding.UTF8.GetBytes($"[{sender}]: {actualMsg}");
                UdpServer._listener.Send(message, message.Length, reciever);
            }
            catch (Exception)
            {
                byte[] reply = Encoding.UTF8.GetBytes("[SERVER] Invalid input.");
                UdpServer._listener.Send(reply, reply.Length, client);
            }
        }

        public void DisplayConnectedClients(IPEndPoint client)
        {
            StringBuilder output = new StringBuilder("[SERVER] Connected users:");

            foreach (string username in UdpServer._all_clients.Keys)
            {
                IPAddress userIP = _clientHandler.GetClientIP(username);
                int userPort = _clientHandler.GetClientPortNum(username);
                output.Append($"\n\t- {username} [{userIP} : {userPort}]");
            }
            output.Append("\n");

            _clientHandler.SendToClient(client, output.ToString());
        }

        public void DisplayOptions(IPEndPoint client)
        {
            StringBuilder output = new StringBuilder("[SERVER] Options:");
            output.Append($"\n\t- CHAT: Select a Chat to chat with users.");
            output.Append($"\n\t- JOIN: Select a GroupChat to chat with users in a group.\n");

            _clientHandler.SendToClient(client, output.ToString());
        }

        public void DisplayGroupChats(IPEndPoint client)
        {
            StringBuilder output = new StringBuilder("[SERVER] To Chat in a group you must join a group first (type: join @'GROUP NAME') ...");
            output.Append("\nAvailable GroupChats:");
            foreach (GroupChat group in UdpServer._group_chats)
            {
                output.Append($"\n\t- {group.Name} ({((!group.IsPrivate) ? "Public" : "Private")})");
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
