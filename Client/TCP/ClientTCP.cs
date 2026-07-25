using Client.Client_Related;
using Client.Clients;
using Client.Events;
using Client.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.TCP
{
    public class ClientTCP : IClient
    {
        // each tcp client is part client part server

        // define public events for ViewModel to subscribe to
        public event EventHandler<MessageRecievedEventArgs> OnMessageReceived;
        public event EventHandler<UserChangedEventArgs> OnUserChanged;
        public event EventHandler<SystemErrorEventArgs> OnSystemError;

        private string _username;
        private TcpListener _listener;
        public string ChatItemName { get; set; } // <- ChatItem name for UI

        private UdpClient _udp_broadcast_listener; // udp broadcast helper to let every user know who is active
        private UdpClient _udp_login_client;

        // tcp handlers
        private TCPLoginHandler _loginHandler;
        private BroadcastHandlerTCP _broadcastHandler;
        private TcpClientHandler _tcp_client_handler;

        // other active users
        private Dictionary<string, IPEndPoint> _users;

        public ClientTCP()
        {
            _users = new Dictionary<string, IPEndPoint>();

            _loginHandler = new TCPLoginHandler();
            _broadcastHandler = new BroadcastHandlerTCP();
            _tcp_client_handler = new TcpClientHandler();

            // event bubbling from Login Handler
            _loginHandler.OnSystemError += (s, e) => OnSystemError?.Invoke(this, e);

            // event bubbling from TcpClientHandler
            _tcp_client_handler.OnSystemError += (s, e) => OnSystemError?.Invoke(this, e);
            _tcp_client_handler.OnMessageReceived += (s, e) => OnMessageReceived?.Invoke(this, e);

            InitTCPListener();
            InitBroadcastHelper();

            Task.Run(Listen);            // <- run TCP listener task in the background
            Task.Run(RecieveBroadcasts); // <- run UDP broadcast listener task in the background

            _udp_login_client = new UdpClient(new IPEndPoint(IPAddress.Any, 0)); // bind to any port and ip
            Task.Run(() => _loginHandler.LoginListener(_udp_login_client));      // <- run login UDP listener task in the background            
        }

        private void InitBroadcastHelper()
        {
            _udp_broadcast_listener = new UdpClient();

            _udp_broadcast_listener.Client.ExclusiveAddressUse = false;
            _udp_broadcast_listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _udp_broadcast_listener.Client.Bind(new IPEndPoint(IPAddress.Any, MulticastGroup.Port));
            MulticastGroup.AddToMulticastGroup(_udp_broadcast_listener);
        }

        private void InitTCPListener()
        {
            // bind to any free port
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
        }


        // <=== IClient methods ===> 

        public bool Connect(string username)
        {
            bool isFree = UsernameAuthorizer.IsFreeUsername(username, _udp_login_client);

            if (isFree)
            {
                _username = username;
                this.ChatItemName = username;

                // broadcast my existence
                BroadcastUsername();

                OnUserChanged?.Invoke(this, new UserChangedEventArgs(username, State.Connecting));
                return true;
            }

            // if taken we kill the new empty client...
            _udp_broadcast_listener.Close();
            _udp_login_client.Close();

            return false;
        }

        /// <summary>
        ///  Trigger safe Disconnect to client who closes his window
        /// </summary>
        private volatile bool _disconnecting = false;
        public void DisconnectClient()
        {
            if (_udp_login_client != null && !string.IsNullOrEmpty(_username))
            {
                _disconnecting = true;
                _loginHandler.Disconnecting = true;

                // dispose of this client's active connections
                _tcp_client_handler.DisposeConnections();

                // broadcast to everyone that this user disconnected
                MulticastGroup.SendToMulticastGroup($"$DISCONNECT_USER_SIGNAL$#{_username}", _udp_login_client);
                OnUserChanged?.Invoke(this, new UserChangedEventArgs(_username, State.Disconnecting));

                // disconnect udp helpers
                _udp_login_client.Close();
                _udp_broadcast_listener.Close();
            }
        }

        public void SendUnicastMessage(string remoteClientName, string message)
        {
            try
            {
                _tcp_client_handler.ConnectAndSend(remoteClientName, _users[remoteClientName], message, _username); // connect and send to the user                              
            }
            catch (Exception e)
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"Couldn't write to user due to: {e.Message}."));
            }
        }

        public List<string> GetActiveUsers() => _users.Keys.ToList();


        /// <summary>
        /// We run a new task for each remote client which tries to contact us because if we would 
        /// handle everything only in this method, the listener won't be able to listen and accept
        /// new clients while we're handling the chat between the original remote client. We need 
        /// to handle each client independently and listen for new clients simultaneously.
        /// </summary>
        private void Listen()
        {
            while (true)
            {
                try
                {
                    TcpClient remote_client = _listener.AcceptTcpClient();
                    Task.Run(() => _tcp_client_handler.HandleRemoteClient(remote_client)); // <- run new Task for every remote user
                }
                catch (Exception e)
                {
                    // if the user is disconnecting don't show system error msg boxes
                    if (!_disconnecting)
                        OnSystemError?.Invoke(this, new SystemErrorEventArgs("Couldn't recieve Unicast message."));
                }
            }
        }


        // <=== UDP broadcast helper methods ===>

        /// <summary>
        /// UDP Broadcast listener that listens for broadcasted messaged on the Multicast group port
        /// </summary>
        private void RecieveBroadcasts()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // listen to any remote user
            while (true)
            {
                try
                {
                    byte[] recievedBytes = _udp_broadcast_listener.Receive(ref remoteEndPoint);
                    ReadBroadcast(recievedBytes, remoteEndPoint);
                }
                catch (Exception e)
                {
                    // if the user is disconnecting don't show system error msg boxes
                    if (!_disconnecting)
                        OnSystemError?.Invoke(this, new SystemErrorEventArgs("Couldn't recieve Broadcast message."));
                }
            }
        }

        private void ReadBroadcast(byte[] recievedBytes, IPEndPoint remoteEndPoint)
        {
            if (CheckUsernameBroadcast(recievedBytes, remoteEndPoint)) return;

            // if 
            string recieved = Encoding.UTF8.GetString(recievedBytes);
            string[] splitted = recieved.Split('#');
            if (splitted.Length >= 2 && splitted[1] == _username) return;

            int executed_option = _broadcastHandler.HandleBroadcast(recievedBytes, ref _users);

            // if new user added, send him my name so he adds me and update UI
            if (executed_option == 1)
            {
                OnUserChanged?.Invoke(this, new UserChangedEventArgs(splitted[1], State.Connecting));
                BroadcastUsername();
            }

            // if user removed, update UI
            else if (executed_option == 2)
            {
                OnUserChanged?.Invoke(this, new UserChangedEventArgs(splitted[1], State.Disconnecting));
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
                    _udp_broadcast_listener.Send(buffer, buffer.Length, remoteEndPoint);
                }
                return true;
            }
            return false;
        }

        private void BroadcastUsername()
        {
            // send username to multicast group so every user will know who is connected and where
            MulticastGroup.SendToMulticastGroup($"$NEW_USER_SIGNAL$#{_username}#{_listener.LocalEndpoint}", _udp_broadcast_listener);
        }
    }
}
