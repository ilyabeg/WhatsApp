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
        // client for unicast
        private UdpClient _client;
        private string _username;

        // listener to listen for broadcasts
        private UdpClient _listener;
        private readonly int _listening_port = 20000;
        public readonly IPEndPoint _listeningEndPoint;

        private Dictionary<string, IPEndPoint> _users;
        private Dictionary<string, Action> _input_option;


        // declare console event and static reference to prevent garbage collection
        private delegate bool ConsoleEventDelegate(int eventType);
        private static ConsoleEventDelegate? _handler;

        // import winAPI dll to use the SetConsoleCtrlHandler method
        [DllImport("kernel32.dll", SetLastError = true)]        
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);


        public ClientUDP()
        {
            _listeningEndPoint = new IPEndPoint(IPAddress.Any, _listening_port);
            InitListener();

            _users = new Dictionary<string, IPEndPoint>();
            
            _client = new UdpClient(new IPEndPoint(IPAddress.Any, 0)); // bind to any port
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

        private void InitListener()
        {
            _listener = new UdpClient();

            _listener.Client.ExclusiveAddressUse = false;
            _listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _listener.Client.Bind(_listeningEndPoint); // bind to multicast port
            MulticastGroup.AddToMulticastGroup(_listener);
        }

        public void InitOptions()
        {
            _input_option = new Dictionary<string, Action>()
            {
                ["CHAT"] = () => {
                    Console.WriteLine("To broadcast specify the destination as 'ALL' ...");
                    Printer.PrintDictKeys("[SYSTEM] Active Users:", _users); 
                    
                    DataPacket packet = Write();

                    if (packet != null && (_users.ContainsKey(packet.Reciever) || packet.Reciever.Equals("all", StringComparison.OrdinalIgnoreCase)))
                        SendDataPacket(packet);
                    else
                        Console.WriteLine("[SYSTEM] Error! Couldn't write the Data Packet.");
                },
                ["CHAT G"] = () =>
                {
                    DataPacket packet = Write();

                    if (packet != null && GroupChats.groupChats.Contains(packet.Reciever))
                        GroupChats.SendToGroupChat(packet, _client);
                    else
                        Console.WriteLine("[SYSTEM] Error! Couldn't write the Data Packet.");
                },
                ["NEW G"] = () =>
                {
                    var newGroupName = GroupChats.CreateNewGroup(_client);

                    // broadcast the new group so all clients add it to their local memmory
                    if (newGroupName != null)
                    {
                        string newGroupBroadcast = $"$ADD_GROUPS_SIGNAL$#{GroupChats.GetGroups()}";
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

            Task.Run(Listen);             // run user listener task in the background
            Task.Run(ListenForBroadcast); // run broadcast listener task in the background

            MulticastGroup.SendToMulticastGroup($"$NEW_USER_SIGNAL$#{_username}#{_client.Client.LocalEndPoint}" , _client);
            Thread.Sleep(250);
            Printer.PrintDictKeys("[SYSTEM] Active Users:", _users);

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
                DataPacket.ProcessDataPacket(recievedBytes);
            }
            catch
            {
                Console.WriteLine("[SYSTEM] Error! Couldn't process Data Packet.");
                Printer.PrintBytes(recievedBytes);
            }
        }

        /// <summary>
        /// Broadcast listener to add/remove users/groups from local memory
        /// </summary>
        private void ListenForBroadcast()
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // listen to any remote user
                while (true)
                {
                    byte[] recievedBytes = _listener.Receive(ref remoteEndPoint);
                    ReadBroadcast(recievedBytes);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SYSTEM] Error! Connection to Network lost due to: {e.Message}");
            }
        }

        private void ReadBroadcast(byte[] recievedBytes)
        {
            try
            {
                int executed_option = BroadcastHandlerUDP.HandleBroadcast(recievedBytes, _users);

                // if broadcast handler couldn't handle the broadcast, try to process it as a Data Packet
                if (executed_option == 0)
                    DataPacket.ProcessDataPacket(recievedBytes);

                // if new user added, send him my name so he knows I exist and all existing group chats.
                if (executed_option == 1)
                {
                    MulticastGroup.SendToMulticastGroup($"$NEW_USER_SIGNAL$#{_username}#{_client.Client.LocalEndPoint}", _client);

                    string existing_groups = GroupChats.GetGroups();
                    if (existing_groups != null)
                        MulticastGroup.SendToMulticastGroup($"$ADD_GROUPS_SIGNAL$#{existing_groups}", _client);
                }
            }
            catch
            {
                Console.WriteLine("[SYSTEM] Error! Couldn't process broadcast.");
                Printer.PrintBytes(recievedBytes);
            }
        }        

        private DataPacket? Write()
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

        /// <summary>
        /// Sends datapacket as bytes to remote user (unicast)
        /// </summary> 
        /// <param name="packet"></param>
        private void SendDataPacket(DataPacket packet)
        {
            string datapacket = JsonSerializer.Serialize(packet);

            if (packet.Reciever.Equals("all", StringComparison.OrdinalIgnoreCase))
                MulticastGroup.SendToMulticastGroup(datapacket, _client);
            else
            {
                byte[] buffer = Encoding.UTF8.GetBytes(datapacket);
                _client.Send(buffer, buffer.Length, _users[packet.Reciever]);
            }            
        }
    }
}
