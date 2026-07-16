using Client.Application;
using Client.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Client.Clients
{
    internal class ClientUDP : IClient
    {
        private UdpClient _client;
        private string _username = "user0";

        private byte[] _buffer;
        private readonly int _bufferSize = 4096;

        private readonly int _listening_port = 20000;
        private Dictionary<string, Action> _input_option;

        public ClientUDP()
        {
            InitClient();            
            InitOptions();

            _buffer = new byte[_bufferSize];
            _username = GetUserName(_username);     
            
            Console.CancelKeyPress += DisconnectEventHandler; // <- attach event upon client disconnection
        }

        private void InitClient()
        {
            _client = new UdpClient();

            _client.Client.ExclusiveAddressUse = false;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _client.Client.Bind(new IPEndPoint(IPAddress.Loopback, _listening_port));
            MulticastGroup.AddToMulticastGroup(_client);
        }

        public void InitOptions()
        {
            _input_option = new Dictionary<string, Action>();
            _input_option.Add("CHAT", () => {
                Console.WriteLine("To broadcast specify the destination as 'ALL' ...");
                DataPacket packet = Write();
                if (packet != null) SendDataPacket(packet);
            });
            _input_option.Add("CHAT G", () => {
                DataPacket packet = Write();
                if (packet != null) GroupChats.SendToGroupChat(packet, _client);
            });
            _input_option.Add("NEW G", () => {
                // create new group
                var newGroup = GroupChats.CreateNewGroup(_client);

                // broadcast the new group so all clients add it to their local memmory
                if (newGroup.groupName != null && newGroup.groupIP != null)
                {
                    string newGroupBroadcast = $"$NEW_GROUP_SIGNAL$#{newGroup.groupName}#{newGroup.groupIP}";
                    MulticastGroup.SendToMulticastGroup(newGroupBroadcast, _client);
                }
            });
            _input_option.Add("JOIN G", () => GroupChats.JoinGroup(_client));
            _input_option.Add("LEAVE G", () => GroupChats.LeaveGroup(_client));
            _input_option.Add("OPTIONS", Printer.PrintOptions);
            _input_option.Add("CLEAR", Console.Clear);
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
            MulticastGroup.SendToMulticastGroup(_username + " is logged in...", _client);

            while (true)
            {
                string message = Console.ReadLine().Trim().ToUpper();
                ProcessMessage(message);
            }
        }

        public void ProcessMessage(string message)
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
                string str = Encoding.UTF8.GetString(recievedBytes);
                if (str.StartsWith("$NEW_GROUP_SIGNAL$"))
                    GroupChats.AddGroup(str);
                else
                    ProcessDataPacket(recievedBytes);
            }
            catch
            {
                Printer.PrintBytes(recievedBytes);
            }
        }

        private void ProcessDataPacket(byte[] recievedBytes)
        {
            DataPacket recievedPacket = DataPacket.TransferData(recievedBytes);

            // if the message is meant for me -> print it, else, ignore it
            if (recievedPacket.Reciever.Equals(_username, StringComparison.OrdinalIgnoreCase) ||
                recievedPacket.Reciever.Equals("all", StringComparison.OrdinalIgnoreCase))
                Printer.PrintDataPacket(recievedPacket);
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
            MulticastGroup.SendToMulticastGroup(datapacket, _client);
        }   

        private void DisconnectEventHandler(object sender, EventArgs e)
        {
            MulticastGroup.SendToMulticastGroup($"{_username} Disconnected...\n", _client);
        }
    }
}
