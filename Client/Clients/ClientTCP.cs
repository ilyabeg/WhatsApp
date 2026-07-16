using Client.Application;
using Client.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.Clients
{
    internal class ClientTCP : IClient
    {
        // each tcp client is part client part server

        private TcpClient _client; // <- client half
        private string _username = "user0";
        private readonly int _client_port = 20000;

        private TcpListener _listener; // <- "server" half
        private NetworkStream _stream;
        
        private byte[] _buffer;
        private readonly int _bufferSize = 4096;

        private UdpClient _broadcast_helper; // udp broadcast helper to let every user know who is active

        private Dictionary<string, IPEndPoint> _users;

        public ClientTCP()
        {
            InitClient();
            InitListener();
            InitBroadcastHelper();
                       
            _buffer = new byte[_bufferSize];
            _username = GetUserName(_username);

            _users = new Dictionary<string, IPEndPoint>();  
            Console.CancelKeyPress += BroadcastDisconnect; // <- attach disconnect event handler
        }

        private void InitClient()
        {
            _client = new TcpClient();

            _client.Client.ExclusiveAddressUse = false;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _client.Client.Bind(new IPEndPoint(IPAddress.Loopback, _client_port));                        
        }

        private void InitBroadcastHelper()
        {
            _broadcast_helper = new UdpClient();

            _broadcast_helper.Client.ExclusiveAddressUse = false;
            _broadcast_helper.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _broadcast_helper.Client.Bind(new IPEndPoint(IPAddress.Any, _client_port));
            MulticastGroup.AddToMulticastGroup(_broadcast_helper);
        }

        private void InitListener()
        {
            // bind to any free port
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
        }

        private string GetUserName(string deafult)
        {
            Console.WriteLine($"Before starting to chat, enter your user name (current deafult: {deafult}):");
            string username = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(username)) 
                return username;

            return deafult;
        }

        public void Start()
        {
            Task.Run(Listen);            // run listen task in the background
            Task.Run(RecieveBroadcasts); // run broadcast reciever task

            BroadcastUsername();

            Console.WriteLine("[SYSTEM] To Start chatting type: '@user' and write a message:");
            Printer.PrintDictKeys("[SYSTEM] Active users:", _users);

            while (true)
            {
                string message = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(message))
                {
                    Console.WriteLine("Enter a valid input.");
                    continue;
                }

                Send(message);
            }
        }

        private void Send(string message)
        {
            try
            {
                string selected_user = StringParser.ParseRemoteUser(message);
                string actualMessage = StringParser.ParseActualMsg(message);

                _client.Connect(_users[selected_user]); // connect to the remote user
                _stream = _client.GetStream();

                byte[] buffer = Encoding.UTF8.GetBytes(actualMessage);
                _stream.Write(buffer, 0, buffer.Length);                
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SYSTEM] Error! Couldn't write to user due to {e.Message}");
            }
        }

        private void Listen()
        {
            try
            {
                int totalRead;
                while ((totalRead = _stream.Read(_buffer, 0, _buffer.Length)) != 0)
                {
                    string recievedMessage = Encoding.UTF8.GetString(_buffer, 0, totalRead);
                    Printer.PrintMessage(recievedMessage);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error! Connection to remote user lost.");
            }
        }

        private void RecieveBroadcasts()
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // listen to any remote user
                while (true)
                {
                    byte[] recievedBytes = _broadcast_helper.Receive(ref remoteEndPoint);
                    ReadBroadcast(recievedBytes, remoteEndPoint);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SYSTEM] Error! Connection to Network lost due to: {e.Message}");
            }
        }

        private void ReadBroadcast(byte[] recievedBytes, IPEndPoint remoteEndPoint)
        {
            string recievedMessage = Encoding.UTF8.GetString(recievedBytes);
            string[] splittedString = recievedMessage.Split('#');

            if (recievedMessage.StartsWith("$USER_SIGNAL$"))
                AddNewUser(splittedString[1], splittedString[2]);

            else if (recievedMessage.StartsWith("$DISCONNECT_SIGNAL$"))
                RemoveUser(splittedString[1]);

            else
                Console.WriteLine($"[SYSTEM] Recieved -> {recievedMessage} from broadcast");
        }

        private void AddNewUser(string username, string endpoint)
        {
            if (!_users.ContainsKey(username))
            {
                string[] splitedEndPoint = endpoint.Split(':');

                IPAddress ip = IPAddress.Parse(splitedEndPoint[0]);
                int port = int.Parse(splitedEndPoint[1]);

                IPEndPoint ep = new IPEndPoint(ip, port);

                _users.TryAdd(username, ep);
            }
        }

        private void RemoveUser(string username)
        {
            if (_users.ContainsKey(username))
            {
                _users.Remove(username);
            }
        }

        private void BroadcastUsername()
        {
            // send username to multicast group so every user will know who is connected and where
            MulticastGroup.SendToMulticastGroup($"$USER_SIGNAL$#{_username}#{_listener.LocalEndpoint}", _broadcast_helper);
        }

        private void BroadcastDisconnect(object sender, EventArgs e)
        {
            // send username to multicast group so every user will know who disconnected
            MulticastGroup.SendToMulticastGroup($"$DISCONNECT_SIGNAL$#{_username}", _broadcast_helper);
        }
    }
}
