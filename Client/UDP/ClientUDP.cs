using Client.Client_Related;
using Client.Clients;
using Client.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Client.UDP
{
    internal class ClientUDP : IClient
    {
        private UdpClient _client;
        private string _username = "user0";

        private readonly int _listening_port = 20000;

        public readonly IPEndPoint _udpEndPoint;
        public readonly IPEndPoint _localEndPoint;

        private static readonly int _buffer_size = 4096;

        private Dictionary<string, IPEndPoint> _users;
        private Dictionary<string, Action> _input_option;

        public ClientUDP()
        {
            _udpEndPoint = new IPEndPoint(IPAddress.Any, _listening_port);
            _localEndPoint = new IPEndPoint(IPAddress.Loopback, _listening_port);

            _users = new Dictionary<string, IPEndPoint>();

            InitClient();
            _username = GetUserName(_username);

            InitOptions();
            
            Console.CancelKeyPress += DisconnectEventHandler; // <- attach event upon client disconnection
        }

        private void InitClient()
        {
            _client = new UdpClient();

            _client.Client.ExclusiveAddressUse = false;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _client.Client.Bind(_udpEndPoint);

            MulticastGroup.AddToMulticastGroup(_client);
        }

        public void InitOptions()
        {
            _input_option = new Dictionary<string, Action>()
            {
                ["CHAT"] = () => {
                    Console.WriteLine("To broadcast specify the destination as 'ALL' ...");
                    DataPacket packet = Write();
                    if (packet != null) SendDataPacket(packet);
                },
                ["CHAT G"] = () =>
                {
                    DataPacket packet = Write();
                    if (packet != null) GroupChats.SendToGroupChat(packet, _client);
                },
                ["NEW G"] = () =>
                {
                    var newGroup = GroupChats.CreateNewGroup(_client);

                    // broadcast the new group so all clients add it to their local memmory
                    if (newGroup.groupName != null && newGroup.groupIP != null)
                    {
                        string newGroupBroadcast = $"$NEW_GROUP_SIGNAL$#{newGroup.groupName}#{newGroup.groupIP}";
                        MulticastGroup.SendToMulticastGroup(newGroupBroadcast, _client);
                    }
                },
                ["JOIN G"] = () => GroupChats.JoinGroup(_client),
                ["LEAVE G"] = () => GroupChats.LeaveGroup(_client),
                ["OPTIONS"] = Printer.PrintOptions,
                ["CLEAR"] = Console.Clear
            };
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
            Printer.PrintOptions();

            Task.Run(Listen); // run listen task in the background     
            MulticastGroup.SendToMulticastGroup($"$NEW_USER_SIGNAL$#{_username}" , _client);

            while (true)
            {
                string message = Console.ReadLine().Trim().ToUpper();
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(string message)
        {
            if (_input_option.ContainsKey(message))
                _input_option[message].Invoke();
            else
                Console.WriteLine("[SYSTEM] Enter valid input.");
        }

        private void Listen()
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // listen to any remote user
                while (true)
                {
                    byte[] recievedBytes = _client.Receive(ref remoteEndPoint);
                    Read(recievedBytes, remoteEndPoint);                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SYSTEM] Error! Connection to Network lost due to: {e.Message}");
            }
        }

        private void Read(byte[] recievedBytes, IPEndPoint remoteEP)
        {
            try
            {
                int executed_option = BroadcastRecieverHandler.HandleBroadcast(recievedBytes, ref _users);

                // if BroadcastHandler couldn't deal with the broadcast, let the data packet processor try to hanlde the data
                if (executed_option == 0)
                    DataPacket.ProcessDataPacket(recievedBytes, _username);

                // if new user added, send him my name so he knows I exist.
                else if (executed_option == 1)
                    SendTo(remoteEP, $"$NEW_USER_SIGNAL$#{_username}");
            }
            catch
            {
                Printer.PrintBytes(recievedBytes);
            }
        }        

        private DataPacket Write()
        {
            try
            {
                DataPacket packet = DataPacket.CreateNew();
                packet.Author = _username; 
                return packet;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SYSTEM] Error! Couldn't write message due to {e.Message}");
            }
            return null;
        }

        private void SendDataPacket(DataPacket packet)
        {
            string datapacket = JsonSerializer.Serialize(packet);
            SendTo(_users[packet.Reciever], datapacket);
        }   

        public void SendTo(IPEndPoint remoteEP, string message)
        {
            byte[] buffer = new byte[_buffer_size];
            buffer = Encoding.UTF8.GetBytes(message);
            _client.Send(buffer, buffer.Length, remoteEP);
        }

        private void DisconnectEventHandler(object sender, EventArgs e)
        {
            MulticastGroup.SendToMulticastGroup($"$DISCONNECT_USER_SIGNAL$#{_username}", _client);
        }
    }
}
