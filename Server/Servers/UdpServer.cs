using Server.Builders;
using Server.Client_Handlers;
using Server.Interfaces;
using Server.IO_Handlers;
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

            UdpOutputHandler outputHandler = new UdpOutputHandler();
            UdpInputHandler inputHandler = new UdpInputHandler(outputHandler);
            _clientHandler = new UdpClientHandler(outputHandler, inputHandler);

            _group_chats = BuildGroupChats();

            Console.WriteLine("[SERVER] Server successfuly initialized.\n");
        }

        public void Run()
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (true)
                {
                    byte[] receiveBytes = _listener.Receive(ref clientEndPoint);
                    string message = Encoding.UTF8.GetString(receiveBytes).Trim();

                    // check if new datapacket belongs to a new user
                    if (IsNewClient(ref clientEndPoint))
                        _clientHandler.HandleNewClient(clientEndPoint, receiveBytes);
                    else
                        _clientHandler.HandleExistingClient(clientEndPoint, message);                                 
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

        private List<GroupChat> BuildGroupChats()
        {
            GroupChatBuilder builder = new GroupChatBuilder();
            List<GroupChat> lst = new List<GroupChat>();

            builder.NewGroup()                
                .SetConfig()
                .SetPort(15000)
                .SetName("Group Chat 1")                
                .SetPrivacy(false);
            lst.Add(builder.Build());

            builder.NewGroup()
                .SetConfig()
                .SetPort(16000)
                .SetName("Group Chat 2")
                .SetPrivacy(false);
            lst.Add(builder.Build());

            builder.NewGroup()
                .SetConfig()
                .SetPort(17000)
                .SetName("Group Chat 3")                
                .SetPrivacy(true);
            lst.Add(builder.Build());

            return lst;
        }
    }
}
