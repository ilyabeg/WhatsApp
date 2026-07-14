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
        public static ConcurrentDictionary<string, GroupChat> _group_chats { get; private set; }

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

            InitGroups();

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
                    string message = Encoding.UTF8.GetString(receiveBytes);

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

        private void InitGroups()
        {
            _group_chats = BuildGroupChats();
            foreach (GroupChat group in _group_chats.Values)
            {
                group.Start();
                Console.WriteLine($"Group chat {group.Name} listening on ep -> {group.EndPoint} ...");
            }
        }

        private ConcurrentDictionary<string, GroupChat> BuildGroupChats()
        {
            GroupChat group;
            GroupChatBuilder builder = new GroupChatBuilder();
            ConcurrentDictionary<string, GroupChat> groups = new ConcurrentDictionary<string, GroupChat>();

            builder.NewGroup()                
                .SetConfig()
                .SetEndPoint(15000, IPAddress.Any)
                .SetName("GroupChat1")                
                .SetPrivacy(false);

            group = builder.Build();
            groups.TryAdd(group.Name, group);

            builder.NewGroup()
                .SetConfig()
                .SetEndPoint(16000, IPAddress.Any)
                .SetName("GroupChat2")
                .SetPrivacy(false);

            group = builder.Build();
            groups.TryAdd(group.Name, group);

            builder.NewGroup()
                .SetConfig()
                .SetEndPoint(17000, IPAddress.Any)
                .SetName("GroupChat3")                
                .SetPrivacy(true);

            group = builder.Build();
            groups.TryAdd(group.Name, group);

            return groups;
        }
    }
}
