using Server.Servers;
using System.Net;
using System.Net.Sockets;

namespace Server.Handlers
{
    internal class InputHandler
    {
        public static void ProcessMessage(string msg, TcpClient client, string sender)
        {
            string recieverID = "#", actualMsg = "#";
            try
            {
                if (msg == null || msg.IsWhiteSpace())
                    throw new Exception();

                recieverID = GetRecieverID(msg.Trim());
                actualMsg = GetActualMsg(msg.Trim());

                TcpClient reciever = TcpServer._all_clients[recieverID];
                ClientHandler.SendToClient(reciever, $"[{sender}]: {actualMsg}");
            }
            catch (Exception e)
            {
                ClientHandler.SendToClient(client, "[SERVER] Invalid input.");
                return;
            }            
        }

        private static string GetRecieverID(string msg)
        {
            int start = msg.IndexOf('@');
            int end = msg.IndexOf(' ');

            if (start != 0 || end == -1) throw new Exception();

            string id = msg.Substring(start + 1, end - 1);

            if (id == null || id.IsWhiteSpace()) throw new Exception();

            return id;
        }

        private static string GetActualMsg(string msg)
        {
            int start = msg.IndexOf(' ');

            if (start == -1) throw new Exception();

            string message = msg.Substring(start + 1);

            if (message == null || message.IsWhiteSpace()) throw new Exception();

            return message;
        }

        public static void PrintMessageDetails(string msg, string clientID)
        {
            if (msg == null || msg.IsWhiteSpace()) return;

            IPAddress clientIP = ClientHandler.GetClientIP(clientID);
            int clientPort = ClientHandler.GetClientPortNum(clientID);
            Console.WriteLine($"[SERVER] Recieved: '{msg}' from {clientID} [{clientIP} : {clientPort}].");
        }
    }
}
