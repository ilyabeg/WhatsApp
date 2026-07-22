using Client.Clients;
using Client.Events;
using Client.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Client.UDP
{
    internal class ClientUDP : IClient
    {
        // define public events for ViewModel to subscribe to
        public event EventHandler<MessageRecievedEventArgs> OnMessageReceived;
        public event EventHandler<UserChangedEventArgs> OnUserChanged;
        public event EventHandler<GroupChangedEventArgs> OnGroupsChanged;
        public event EventHandler<SystemErrorEventArgs> OnSystemError;

        // client for unicast
        private UdpClient _client;
        private string _username;

        // other active users
        private Dictionary<string, IPEndPoint> _users;

        // local group chats manager
        private GroupChats _groupsManager;

        // broadcast helper
        BroadcastHandlerUDP _broadcastHandler;

        // listener to listen for broadcasts
        private UdpClient _listener;
        private readonly int _listening_port = 20000;
        public readonly IPEndPoint _listeningEndPoint;

        // declare console event and static reference to prevent garbage collection
        private delegate bool ConsoleEventDelegate(int eventType);
        private static ConsoleEventDelegate? _handler;

        // import winAPI dll to use the SetConsoleCtrlHandler method
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        public ClientUDP()
        {
            _users = new Dictionary<string, IPEndPoint>();
            _groupsManager = new GroupChats();
            _broadcastHandler = new BroadcastHandlerUDP();

            // event bubbling
            _groupsManager.OnSystemError += (s, e) => OnSystemError?.Invoke(this, e);
            _groupsManager.OnGroupChange += (s, e) => OnGroupsChanged?.Invoke(this, e);

            _listeningEndPoint = new IPEndPoint(IPAddress.Any, _listening_port);
            InitListener();

            Task.Run(ListenForBroadcast); // run broadcast listener task in the background
            Task.Run(Listen); // run user listener task in the background

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

        public void Connect(string username)
        {
            _username = username;
            _client = new UdpClient(new IPEndPoint(IPAddress.Any, 0)); // bind to any port and ip
            OnUserChanged?.Invoke(this, new UserChangedEventArgs(username, State.Connecting));
        }

        public void SendUnicastMessage(string remoteClientName, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            _client.Send(buffer, buffer.Length, _users[remoteClientName]);
        }        


        // <=== Group chats methods ===>

        public void CreateGroup(string name)
        {
            _groupsManager.CreateNewGroup(_client, name);
        }

        public void SendGroupMessage(string name, string message)
        {
            _groupsManager.SendToGroup(_client, _username, name, message);
        }

        public void JoinGroup(string name)
        {
            _groupsManager.JoinGroup(_client, name);
        }

        public void LeaveGroup(string name)
        {
            _groupsManager.LeaveGroup(_client, name);
        }

        /// <summary>
        /// Listens for incoming unicast messages
        /// </summary>
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
                OnSystemError?.Invoke(this, new SystemErrorEventArgs("Couldn't recieve Unicast message."));
            }
        }

        private void Read(byte[] recievedBytes, IPEndPoint remoteEP)
        {
            try
            {
                string message = Encoding.UTF8.GetString(recievedBytes);
                OnMessageReceived?.Invoke(this, new MessageRecievedEventArgs(RemoteClientAt(remoteEP), message));
            }
            catch
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs("Couldn't process recieved Message."));
            }
        }

        /// <summary>
        /// Returns the UserName of the remote client that is at the provided remote endpoint
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <returns></returns>
        private string? RemoteClientAt(IPEndPoint remoteEP)
        {
            foreach (string username in _users.Keys)
            {
                if (_users[username].Equals(remoteEP)) return username;
            }
            return null;
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
                    ReadBroadcast(recievedBytes, remoteEndPoint);
                }
            }
            catch (Exception)
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs("Connection to network lost."));
            }
        }

        private void ReadBroadcast(byte[] receivedBytes, IPEndPoint remoteEndPoint)
        {
            try
            {
                if (CheckUsernameBroadcast(receivedBytes, remoteEndPoint)) return;

                int executed_option = _broadcastHandler.HandleBroadcast(receivedBytes, remoteEndPoint, _users, _groupsManager);
                // if broadcast handler couldn't handle the broadcast, try to process it as a Data Packet
                if (executed_option == 0)
                {
                    DataPacket packet = DataPacket.TransferData(receivedBytes);
                    if (packet != null)
                    {
                        OnMessageReceived?.Invoke(this, new MessageRecievedEventArgs(packet.Author, packet.Message));
                    }
                }

                // if new user added
                else if (executed_option == 1)
                {
                    // notify UI the users list
                    OnUserChanged?.Invoke(this, new UserChangedEventArgs(RemoteClientAt(remoteEndPoint), State.Connecting));

                    // send to new user my name so he knows I exist and all existing group chats.
                    MulticastGroup.SendToMulticastGroup($"$NEW_USER_SIGNAL$#{_username}", _client);

                    string existing_groups = _groupsManager.GetGroups();
                    if (existing_groups != null)
                        MulticastGroup.SendToMulticastGroup($"$ADD_GROUPS_SIGNAL$#{existing_groups}", _client);
                }

                // if user disconnected
                else if (executed_option == 2)
                {
                    OnUserChanged?.Invoke(this, new UserChangedEventArgs(RemoteClientAt(remoteEndPoint), State.Disconnecting));
                }
            }
            catch 
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs("Couldn't recieve Unicast message."));
            }
        }

        /// <summary>
        /// Checks to see if the broadcast was the USERNAME CHECK SIGNAL and return true if so, else returns false
        /// </summary>
        /// <param name="receivedBytes"></param>
        /// <returns></returns>
        private bool CheckUsernameBroadcast(byte[] receivedBytes, IPEndPoint remoteEndPoint)
        {
            string str = Encoding.UTF8.GetString(receivedBytes);
            if (str.StartsWith("$CHECK_USERNAME_SIGNAL$"))
            {
                string[] splitted = str.Split('#');
                string username = splitted[1];

                // tell client that the username taken
                if (username.Equals(_username))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes("$USERNAME_IS_TAKEN$");
                    _client.Send(buffer, buffer.Length, remoteEndPoint);
                }

                return true;
            }
            return false;
        }
    }
}
