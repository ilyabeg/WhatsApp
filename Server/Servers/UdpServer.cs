using Server.Builders;
using Server.Client_Handlers;
using Server.InputHandlers;
using Server.Interfaces;
using Server.Objects;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Servers
{
    internal class UdpServer : IServer
    {
        public static UdpClient _listener { get; private set; }
        public static ConcurrentDictionary<string, IPEndPoint> _all_clients; // connected clients by their id
        private readonly int _listeningPortNumber = 14000;

        private static UdpIOHandler _ioHandler;
        private UdpClientHandler _clientHandler;

        public static List<GroupChat> _group_chats { get; private set; }


        public UdpServer()
        {
            InitServer();
        }

        private void InitServer()
        {
            _all_clients = new ConcurrentDictionary<string, IPEndPoint>();
            _listener = new UdpClient(_listeningPortNumber);
            _ioHandler = new UdpIOHandler();
            _clientHandler = new UdpClientHandler();
            _group_chats = BuildGroupChats();

            Console.WriteLine("[SERVER] Server successfuly initialized.\n");
        }

        public void Run()
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //_ioHandler.DisplayConnectedClients(clientEndPoint);
            try
            {
                while (true)
                {
                    byte[] receiveBytes = _listener.Receive(ref clientEndPoint);
                    string message = Encoding.UTF8.GetString(receiveBytes).Trim();

                    string clientID;

                    // check if new datapacket belongs to a new user
                    if (IsNewClient(ref clientEndPoint))
                    {
                        clientID = GetClientID(receiveBytes);
                        _clientHandler.AddClient(clientID, clientEndPoint);
                        _ioHandler.DisplayOptions(clientEndPoint);
                    }
                    else
                    {           
                        clientID = _clientHandler.GetClientID(ref clientEndPoint);
                        _ioHandler.PrintMessageDetails(message, clientID);

                        if (message.Equals("join", StringComparison.OrdinalIgnoreCase))
                            _ioHandler.DisplayGroupChats(clientEndPoint);

                        else if (message.Equals("chat", StringComparison.OrdinalIgnoreCase))
                            _ioHandler.DisplayConnectedClients(clientEndPoint);

                        else
                            _ioHandler.HandleMessage(message, clientEndPoint, clientID);
                    }                                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error! Server Crashed due to: {e.Message}");
                _listener.Close();
            }
        }

        private bool IsNewClient(ref IPEndPoint clientEndPoint)
        {
            foreach (IPEndPoint endPoint in _all_clients.Values)
            {
                if (endPoint.Equals(clientEndPoint)) return false;
            }
            return true;
        }

        private string GetClientID(byte[] recievedBytes)
        {
            string username = Encoding.UTF8.GetString(recievedBytes);
            return username;
        }

        private List<GroupChat> BuildGroupChats()
        {
            GroupChatBuilder builder = new GroupChatBuilder();
            List<GroupChat> lst = new List<GroupChat>();

            builder.NewGroup()
                .SetName("Group Chat 1")
                .SetPort(15000)
                .SetPrivacy(false);
            lst.Add(builder.Build());

            builder.NewGroup()
                .SetName("Group Chat 2")
                .SetPort(16000)
                .SetPrivacy(false);
            lst.Add(builder.Build());

            builder.NewGroup()
                .SetName("Group Chat 3")
                .SetPort(17000)
                .SetPrivacy(true);
            lst.Add(builder.Build());

            return lst;
        }
    }
}
