using Client.Client_Related;
using Client.Clients;
using Client.Events;
using Client.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.UDP
{
    public class ClientUDP : IClient
    {
        // define public events for ViewModel to subscribe to
        public event EventHandler<MessageRecievedEventArgs> OnMessageReceived;
        public event EventHandler<UserChangedEventArgs> OnUserChanged;
        public event EventHandler<GroupChangedEventArgs> OnGroupsChanged;
        public event EventHandler<SystemErrorEventArgs> OnSystemError;

        // client for unicast
        private UdpClient _client;
        private string _username;
        public string ChatItemName { get; set; } // <- ChatItem name for UI

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

        public ClientUDP()
        {
            _client = new UdpClient(new IPEndPoint(IPAddress.Any, 0)); // bind to any port and ip
            Task.Run(Listen); // run user listener task in the background

            _users = new Dictionary<string, IPEndPoint>();
            _groupsManager = new GroupChats();
            _broadcastHandler = new BroadcastHandlerUDP();

            // event bubbling from group chats
            _groupsManager.OnSystemError += (s, e) => OnSystemError?.Invoke(this, e);
            _groupsManager.OnGroupChange += OnGroupChatsChanged;

            // event bubbling from broadcast handler
            _broadcastHandler.OnMessageReceived += (s, e) => OnMessageReceived?.Invoke(this, e);
            _broadcastHandler.OnUserChanged += (s, e) => OnUserChanged?.Invoke(this, e);
            _broadcastHandler.OnGroupsChanged += (s, e) => OnGroupsChanged?.Invoke(this, e);

            _listeningEndPoint = new IPEndPoint(IPAddress.Any, _listening_port);
            InitListener();

            Task.Run(ListenForBroadcast); // run broadcast listener task in the background
        }        
        private void InitListener()
        {
            _listener = new UdpClient();

            _listener.Client.ExclusiveAddressUse = false;
            _listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _listener.Client.Bind(_listeningEndPoint); // bind to multicast port
            MulticastGroup.AddToMulticastGroup(_listener);
        }
        /// <summary>
        /// If local groups have changed, invoke UI event to update UI and broadcast to all active connections so they update UI too
        /// </summary>
        private void OnGroupChatsChanged(object sender, GroupChangedEventArgs e)
        {
            OnGroupsChanged?.Invoke(this, e);

            string? existing_groups = _groupsManager.GetGroups();
            if (existing_groups != null)
            {
                // broadcast the updated groups list to everyone
                MulticastGroup.SendToMulticastGroup($"$ADD_GROUPS_SIGNAL$#{existing_groups}", _client);
            }
        }


        // <=== IClient methods ===>

        /// <summary>
        /// Returns true upon Connect succession, false if couldn't connect
        /// </summary>
        /// <param name="username"></param>
        public bool Connect(string username)
        {            
            bool isFree = UsernameAuthorizer.IsFreeUsername(username, _client);

            if (isFree)
            {
                _username = username;
                this.ChatItemName = username;

                // broadcast my existence
                MulticastGroup.SendToMulticastGroup($"$NEW_USER_SIGNAL$#{_username}", _client);

                OnUserChanged?.Invoke(this, new UserChangedEventArgs(username, State.Connecting));
                return true; 
            }

            // if taken we kill the new empty client...
            _client.Close();
            _listener.Close();

            return false;
        }

        /// <summary>
        ///  Trigger safe Disconnect to client who closes his window
        /// </summary>
        private volatile bool _disconnecting = false;
        public void DisconnectClient()
        {
            if (_client != null && !string.IsNullOrEmpty(_username))
            {
                _disconnecting = true;

                // leave all connected groups
                List<string> myGroups = _groupsManager.groupChats.Keys.ToList();
                foreach (string group in myGroups)
                {
                    LeaveGroup(group);
                }

                // broadcast to everyone that this user disconnected
                MulticastGroup.SendToMulticastGroup($"$DISCONNECT_USER_SIGNAL$#{_username}", _client);                

                // disconnect client
                _client.Close();
                _listener.Close();
            }
        }

        public void SendUnicastMessage(string remoteClientName, string message)
        {
            if (!string.IsNullOrWhiteSpace(message) && _users.TryGetValue(remoteClientName, out IPEndPoint remoteEndPoint))
            {
                string sendingMessage = $"{_username}#{message.Trim()}";

                byte[] buffer = Encoding.UTF8.GetBytes(sendingMessage);
                _client.Send(buffer, buffer.Length, remoteEndPoint);
            }
            else
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs("Couldn't send Unicast message."));
            }
        }

        public void SendBroadcast(string message)
        {
            if (!string.IsNullOrWhiteSpace(message) && message.Trim().StartsWith("@all"))
            {
                string sendingMessage = $"{_username}#{message.Trim()}";
                MulticastGroup.SendToMulticastGroup(sendingMessage, _client);
            }
            else
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs("Couldn't send Broadcast message."));
            }
        }

        public List<string> GetActiveUsers() => _users.Keys.ToList();



        // <=== Group chats methods ===>

        public void SendGroupMessage(string groupName, string message)
        {
            _groupsManager.SendToGroup(_client, _username, groupName, message);
        }

        public void CreateGroup(string groupName)
        {
            _groupsManager.CreateNewGroup(_listener, groupName);
        }
        
        public void JoinGroup(string groupName)
        {
            _groupsManager.JoinGroup(_listener, groupName);
        }

        public void LeaveGroup(string groupName)
        {
            _groupsManager.LeaveGroup(_listener, groupName);
        }


        // <=== Client listening ===>

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
            catch
            {
                // if the user is disconnecting don't show system error msg boxes
                if (!_disconnecting)
                    OnSystemError?.Invoke(this, new SystemErrorEventArgs("Couldn't recieve Unicast message."));
            }
        }

        private void Read(byte[] recievedBytes, IPEndPoint remoteEP)
        {
            try
            {
                string message = Encoding.UTF8.GetString(recievedBytes);

                if (message.Equals("$USERNAME_IS_TAKEN$"))
                {
                    UsernameAuthorizer.FreeUsername = false; // change free flag
                    return;
                }
                else
                {
                    string[] splitted = message.Split('#');

                    if (splitted.Length == 2)
                    {
                        string sender = splitted[0];
                        string actualMessage = splitted[1];

                        OnMessageReceived?.Invoke(this, new MessageRecievedEventArgs(sender, actualMessage));
                    }
                }                    
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


        // <=== Broadcast listening ===>

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
            catch
            {
                if (!_disconnecting)
                    OnSystemError?.Invoke(this, new SystemErrorEventArgs("Connection to network lost."));
            }
        }

        private void ReadBroadcast(byte[] receivedBytes, IPEndPoint remoteEndPoint)
        {
            try
            {
                // ignore my own broadcasts
                if (MyBroadcast(receivedBytes, remoteEndPoint)) return;

                int executed_option = _broadcastHandler.HandleBroadcast(receivedBytes, remoteEndPoint, _users, _groupsManager);

                // if new user added
                if (executed_option == 1)
                {
                    // notify UI the users list
                    OnUserChanged?.Invoke(this, new UserChangedEventArgs(RemoteClientAt(remoteEndPoint), State.Connecting));

                    // send to new user my name so he knows I exist and all existing group chats.
                    MulticastGroup.SendToMulticastGroup($"$NEW_USER_SIGNAL$#{_username}", _client);

                    string existing_groups = _groupsManager.GetGroups();
                    if (existing_groups != null)
                        MulticastGroup.SendToMulticastGroup($"$ADD_GROUPS_SIGNAL$#{existing_groups}", _client);
                }
            }
            catch 
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs("Couldn't recieve Broadcast message."));
            }
        }
        
        private bool MyBroadcast(byte[] receivedBytes, IPEndPoint remoteEndPoint)
        {
            // if username check broadcast no need to continue to the broadcast handler
            if (CheckUsernameBroadcast(receivedBytes, remoteEndPoint)) return true;

            string str = Encoding.UTF8.GetString(receivedBytes);
            string[] splitted = str.Split('#', 4);

            if (splitted.Length >= 2)
            {
                if (!str.StartsWith('$') && splitted[0].Equals(_username)) return true; // not a signal just a simple message

                // signal sent by me
                if ((str.StartsWith("$NEW_USER_SIGNAL$") ||
                     str.StartsWith("$DISCONNECT_USER_SIGNAL$") ||
                     str.StartsWith("$ADD_GROUPS_SIGNAL$")) &&
                     splitted[1].Equals(_username)) 
                    return true;

                // group message and sent by me
                if (str.StartsWith("$GROUP_MESSAGE$") && splitted.Length >= 3 && splitted[2].Equals(_username)) return true;
            }
            return false;
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
