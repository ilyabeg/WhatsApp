using Server.Objects;
using Server.Servers;
using System.Net;
using System.Text;

namespace Server.IO_Handlers
{
    internal class UdpOutputHandler
    {
        public void DisplayConnectedClients(IPEndPoint clientEP)
        {
            StringBuilder output = new StringBuilder("[SERVER] Connected users:");

            foreach (string username in UdpServer._all_clients.Keys)
            {
                IPAddress userIP = UdpServer._all_clients[username].Address;
                int userPort = UdpServer._all_clients[username].Port;
                output.Append($"\n\t- {username} [{userIP} : {userPort}]");
            }
            output.Append("\n");
            SendMessage(clientEP, output.ToString());
        }

        public void DisplayOptions(IPEndPoint clientEP)
        {
            StringBuilder output = new StringBuilder("[SERVER] Options:");
            output.Append($"\n\t- CHAT: Select a Chat to chat with users.");
            output.Append($"\n\t- JOIN: Select a GroupChat to chat with users in a group.\n");
            SendMessage(clientEP, output.ToString());
        }

        public void DisplayGroupChats(IPEndPoint clientEP)
        {
            StringBuilder output = new StringBuilder("[SERVER] To Chat in a group you must join a group first (type: join @'GROUP NAME') ...");
            output.Append("\nAvailable GroupChats:");
            foreach (GroupChat group in UdpServer._group_chats)
            {
                output.Append($"\n\t- {group.Name} ({((!group.IsPrivate) ? "Public" : "Private")})");
            }
            output.Append("\n");
            SendMessage(clientEP, output.ToString());
        }

        public void PrintMessageDetails(string msg, IPEndPoint clientEP, string clientID)
        {
            if (msg == null || msg.IsWhiteSpace()) return;
            IPAddress clientIP = clientEP.Address;
            int clientPort = clientEP.Port;
            Console.WriteLine($"[SERVER] Recieved: '{msg}' from {clientID} [{clientIP} : {clientPort}].");
        }

        public void SendInvalidMessage(IPEndPoint clientEP)
        {
            byte[] reply = Encoding.UTF8.GetBytes("[SERVER] Invalid input.");
            UdpServer._listener.Send(reply, reply.Length, clientEP);
        }

        public void SendMessage(IPEndPoint clientEP, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            UdpServer._listener.Send(buffer, buffer.Length, clientEP);
        }
    }
}
