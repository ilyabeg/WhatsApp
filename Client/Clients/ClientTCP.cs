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
        private readonly int _client_port = 21000;

        private TcpListener _listener; // <- "server" half
        private NetworkStream _stream;
        
        private byte[] _buffer;
        private readonly int _bufferSize = 4096;

        private UdpClient _broadcast_helper; // udp broadcast helper to let every user know who is active

        public ClientTCP()
        {
            InitClient();
            InitListener();
            InitBroadcastHelper();
           
            _stream = _client.GetStream();
            _buffer = new byte[_bufferSize];
            _username = GetUserName(_username);
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

            _broadcast_helper.Client.Bind(new IPEndPoint(IPAddress.Loopback, _client_port));
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
            Console.WriteLine("To Start chatting type: '@user' and write a message:");
            Task.Run(Listen); // run listen task in the background 

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
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            _stream.Write(buffer, 0, buffer.Length);
        }

        public void Listen()
        {
            try
            {
                int totalRead;
                while ((totalRead = _stream.Read(_buffer, 0, _buffer.Length)) != 0)
                {
                    string recievedMessage = Encoding.UTF8.GetString(_buffer, 0, totalRead);
                    Read(recievedMessage);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error! Connection to remote user lost.");
            }
        }

        private void Read(string recievedMessage)
        {
            PrintMessage(recievedMessage);
        }

        private void PrintMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
