using Client.Client_Related;
using Client.Clients;
using Client.Events;
using Client.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Client.TCP
{
    public class ClientTCP : IClient
    {
        // each tcp client is part client part server

        private string _username;
        private TcpListener _listener;     
        
        private UdpClient _broadcast_helper; // udp broadcast helper to let every user know who is active
        private UdpClient _login_client;     // udp broadcast helper to let every user know who is active

        private Dictionary<string, IPEndPoint> _users;


        // declare console event and static reference to prevent garbage collection
        private delegate bool ConsoleEventDelegate(int eventType);
        private static ConsoleEventDelegate? _handler;

        public string ChatItemName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler<MessageRecievedEventArgs> OnMessageReceived;
        public event EventHandler<UserChangedEventArgs> OnUserChanged;
        public event EventHandler<GroupChangedEventArgs> OnGroupsChanged;
        public event EventHandler<SystemErrorEventArgs> OnSystemError;

        // import winAPI dll to use the SetConsoleCtrlHandler method
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        public void SendUnicastMessage(string remoteClientName, string message)
        {
            throw new NotImplementedException();
        }

        public bool Connect(string username)
        {
            throw new NotImplementedException();
        }

        public void DisconnectClient()
        {

        }

        public List<string> GetActiveUsers()
        {
            return _users.Keys.ToList();
        }

        public ClientTCP()
        {
            //_users = new Dictionary<string, IPEndPoint>();

            //InitListener();
            //InitBroadcastHelper();

            //Task.Run(Listen);            // <- run TCP listener task in the background
            //Task.Run(RecieveBroadcasts); // <- run UDP broadcast listener task in the background

            //_login_client = new UdpClient(new IPEndPoint(IPAddress.Any, 0)); // bind to any port and ip
            //Task.Run(() => TCPLoginHandler.LoginListener(_login_client));    // <- run login UDP listener task in the background            

            //_username = UsernameAuthorizer.GetUsername(_login_client);            

            //// make new event delegate that runs the event callback 
            //_handler = new ConsoleEventDelegate(ConsoleEventCallback);
            //SetConsoleCtrlHandler(_handler, true);
        }

        ///// <summary>
        ///// event callback that executes this code upon closing the console application using CTRL+C or X button
        ///// </summary>
        ///// <param name="eventType"></param>
        ///// <returns></returns>
        //private bool ConsoleEventCallback(int eventType)
        //{
        //    // 2 represents CTRL_CLOSE_EVENT (the X button)
        //    // 0 represents CTRL+C 
        //    if (eventType == 2 || eventType == 0)
        //    {   
        //        // broadcast to everyone that this user disconnected
        //        MulticastGroup.SendToMulticastGroup($"$DISCONNECT_USER_SIGNAL$#{_username}#{_listener.LocalEndpoint}", _broadcast_helper);
        //        TcpClientHandler.DisposeConnections(); // dispose all of the connections to this client
        //    }

        //    // return false to let normal OS termination continue
        //    return false;
        //}


        //private void InitBroadcastHelper()
        //{
        //    _broadcast_helper = new UdpClient();

        //    _broadcast_helper.Client.ExclusiveAddressUse = false;
        //    _broadcast_helper.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        //    _broadcast_helper.Client.Bind(new IPEndPoint(IPAddress.Any, MulticastGroup.Port));
        //    MulticastGroup.AddToMulticastGroup(_broadcast_helper);
        //}

        //private void InitListener()
        //{
        //    // bind to any free port
        //    _listener = new TcpListener(IPAddress.Loopback, 0);            
        //    _listener.Start();
        //}

        //public void Start()
        //{            
        //    BroadcastUsername();
        //    Thread.Sleep(250);

        //    Console.WriteLine("[SYSTEM] To Start chatting type: '@user' and write a message:");
        //    Printer.PrintDictKeys("[SYSTEM] Active users:", _users);

        //    while (true)
        //    {
        //        string message = Console.ReadLine();

        //        if (string.IsNullOrWhiteSpace(message))
        //        {
        //            Console.WriteLine("Enter a valid input.");
        //            continue;
        //        }

        //        Send(message);
        //    }
        //}

        //private void Send(string message)
        //{
        //    try
        //    {
        //        string selected_user = StringParser.ParseRemoteUser(message);
        //        string actualMessage = StringParser.ParseActualMsg(message);
        //        TcpClientHandler.ConnectAndSend(selected_user, _users[selected_user], actualMessage, _username); // connect and send to the user                              
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"[SYSTEM] Error! Couldn't write to user due to: {e.Message}");
        //    }
        //}        

        ///// <summary>
        ///// We run a new task for each remote client which tries to contact us because if we would 
        ///// handle everything only in this method, the listener won't be able to listen and accept
        ///// new clients while we're handling the chat between the original remote client. We need 
        ///// to handle each client independently and listen for new clients simultaneously.
        ///// </summary>
        //private void Listen()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            TcpClient remote_client = _listener.AcceptTcpClient();
        //            Task.Run(() => TcpClientHandler.HandleRemoteClient(remote_client)); // <- run new Task for every remote user
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine($"Error! Listener crashed due to: {e.Message}");
        //        }
        //    }            
        //}

        ///// <summary>
        ///// UDP Broadcast listener that listens for broadcasted messaged on the Multicast group port
        ///// </summary>
        //private void RecieveBroadcasts()
        //{
        //    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // listen to any remote user
        //    while (true)
        //    {
        //        try
        //        {
        //            byte[] recievedBytes = _broadcast_helper.Receive(ref remoteEndPoint);
        //            ReadBroadcast(recievedBytes, remoteEndPoint);
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine($"[SYSTEM] Error reading broadcast due to: {e.Message}");
        //        }
        //    }
        //}

        //private void ReadBroadcast(byte[] recievedBytes, IPEndPoint remoteEndPoint)
        //{
        //    if (CheckUsernameBroadcast(recievedBytes, remoteEndPoint)) return;
           
        //    int executed_option = BroadcastHandlerTCP.HandleBroadcast(recievedBytes, ref _users);

        //    // if BroadcastHandler couldn't handle the broadcast, print it out
        //    if (executed_option == 0)
        //    {
        //        string recievedMessage = Encoding.UTF8.GetString(recievedBytes);
        //        Console.WriteLine($"[SYSTEM] Recieved -> {recievedMessage} from broadcast.");
        //    }            

        //    if (executed_option == 1)
        //        BroadcastUsername();
        //}

        ///// <summary>
        ///// Checks to see if the broadcast was the USERNAME CHECK SIGNAL and return true if so, else returns false
        ///// </summary>
        ///// <param name="receivedBytes"></param>
        ///// <returns></returns>
        //private bool CheckUsernameBroadcast(byte[] receivedBytes, IPEndPoint remoteEndPoint)
        //{
        //    string str = Encoding.UTF8.GetString(receivedBytes);
        //    if (str.StartsWith("$CHECK_USERNAME_SIGNAL$"))
        //    {
        //        string[] splitted = str.Split('#');
        //        string username = splitted[1];

        //        // tell client that the username taken
        //        if (username.Equals(_username))
        //        {
        //            byte[] buffer = Encoding.UTF8.GetBytes("$USERNAME_IS_TAKEN$");
        //            _broadcast_helper.Send(buffer, buffer.Length, remoteEndPoint);
        //        }

        //        return true;
        //    }
        //    return false;
        //}

        //private void BroadcastUsername()
        //{
        //    // send username to multicast group so every user will know who is connected and where
        //    MulticastGroup.SendToMulticastGroup($"$NEW_USER_SIGNAL$#{_username}#{_listener.LocalEndpoint}", _broadcast_helper);
        //}
    }
}
