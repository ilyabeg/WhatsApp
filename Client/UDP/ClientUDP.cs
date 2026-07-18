using Client.Client_Related;
using Client.Clients;
using Client.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Client.UDP
{
    internal class ClientUDP : IClient
    {
        private UdpClient _client;
        private string _username;

        private readonly int _listening_port = 20000;

        public readonly IPEndPoint _udpEndPoint;

        private List<string> _users;
        private Dictionary<string, Action> _input_option;


        // declare console event and static reference to prevent garbage collection
        private delegate bool ConsoleEventDelegate(int eventType);
        private static ConsoleEventDelegate? _handler;

        // import winAPI dll to use the SetConsoleCtrlHandler method
        [DllImport("kernel32.dll", SetLastError = true)]        
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);


        public ClientUDP()
        {
            _udpEndPoint = new IPEndPoint(IPAddress.Any, _listening_port);

            _users = new List<string>();

            InitClient();
            _username = GetUserName();

            InitOptions();

            // make new event delegate that runs the event callback 
            _handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(_handler, true);
        }

        /// <summary>
        /// event callback that executes this code upon closing the console application using CTRL+C or X button
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private bool ConsoleEventCallback(int eventType)
        {
            // 2 represents CTRL_CLOSE_EVENT (the X button)
            // 0 represents CTRL+C 
            if (eventType == 2 || eventType == 0)
            {
                // broadcast to everyone that this user disconnected
                MulticastGroup.SendToMulticastGroup($"$DISCONNECT_USER_SIGNAL$#{_username}", _client);
            }

            // return false to let normal OS termination continue
            return false;
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
                    Printer.PrintList("[SYSTEM] Active Users:", _users);                    
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

        private string GetUserName()
        {
            Console.WriteLine("[SYSTEM] Before starting to chat, enter your user name:");
            string username = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("[SYSTEM] Please enter valid username:");
                username = Console.ReadLine();
            }
            return username;
        }

        public void Start()
        {
            Printer.PrintOptions();

            Task.Run(Listen); // run listen task in the background     
            MulticastGroup.SendToMulticastGroup($"$NEW_USER_SIGNAL$#{_username}" , _client);
            Thread.Sleep(250);
            Printer.PrintList("[SYSTEM] Active Users:", _users);

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
                    MulticastGroup.SendToMulticastGroup($"$NEW_USER_SIGNAL$#{_username}", _client);
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
            MulticastGroup.SendToMulticastGroup(datapacket, _client);
        }
    }
}
