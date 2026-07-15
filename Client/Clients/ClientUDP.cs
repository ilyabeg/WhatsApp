using Client.Interfaces;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.Clients
{
    internal class ClientUDP : IClient
    {
        private UdpClient _client;
        private byte[] _buffer;
        private string _username = "user0";
        private readonly int _bufferSize = 4096;
        private readonly IPAddress _multicast_group_ip = IPAddress.Parse("239.1.1.1");
        private readonly int _listening_port = 20000;
        private readonly IPEndPoint _multicast_group_ep;

        private ConcurrentDictionary<IPEndPoint, string> _users_by_endpoint; // other users mapped to their endpoint
        private ConcurrentDictionary<string, IPEndPoint> _users_by_name;     // other users mapped to their username

        public ClientUDP()
        {
            _client = new UdpClient();

            _client.Client.ExclusiveAddressUse = false;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _client.Client.Bind(new IPEndPoint(IPAddress.Any, _listening_port));
            _client.JoinMulticastGroup(_multicast_group_ip);

            _buffer = new byte[_bufferSize];
            _username = GetUserName(_username);            
            _multicast_group_ep = new IPEndPoint(_multicast_group_ip, _listening_port);

            _users_by_endpoint = new ConcurrentDictionary<IPEndPoint, string>();
            _users_by_name = new ConcurrentDictionary<string, IPEndPoint>();
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
            Console.WriteLine("To Chat type: '@user' and write down a message...");
            Console.WriteLine("NOTE: Type 'CLEAR' to clear the screen at any time\n");

            Task.Run(Listen); // run listen task in the background     
            SendToMulticastGroup(_username); // send over username to all other devices

            while (true)
            {
                string message = Console.ReadLine().Trim();

                if (string.IsNullOrWhiteSpace(message))
                    continue;

                else if (message.Equals("CLEAR", StringComparison.OrdinalIgnoreCase))
                    Console.Clear();

                else
                    Write(message);
            }
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
                Console.WriteLine($"Error! Connection to Network lost due to: {e.Message}");
            }
        }

        private void Read(byte[] recievedBytes, IPEndPoint remoteEP)
        {           
            string recieved = Encoding.UTF8.GetString(recievedBytes);
            ProcessClient(recieved, remoteEP);    
            
            recieved = $"({_users_by_endpoint[remoteEP]}): {recieved}";
            PrintMessage(recieved);
        }

        private void ProcessClient(string recieved, IPEndPoint remoteEP)
        {
            // add to users if new client
            if (!_users_by_endpoint.ContainsKey(remoteEP))
            {
                _users_by_endpoint.TryAdd(remoteEP, recieved);
                ProcessUsername(recieved, remoteEP);
            }
        }

        private void ProcessUsername(string recieved, IPEndPoint remoteEP)
        {
            if (_users_by_name.ContainsKey(recieved))
            {
                // add hash to make username uniqe
                string hash = recieved.GetHashCode().ToString();
                recieved = recieved + hash.Substring(hash.Length - 4);
            }
            _users_by_name.TryAdd(recieved, remoteEP);
        }

        private void PrintMessage(string message)
        {
            Console.WriteLine("Recieved -> " + message);
        }

        private void Write(string message)
        {
            try
            {
                IPEndPoint remoteEP = GetRemoteEPFromMessage(ref message); // parse remote user from message
                _buffer = Encoding.UTF8.GetBytes(message);
                _client.Send(_buffer, _buffer.Length, remoteEP);
            }
            catch
            {
                Console.WriteLine("Error! Couldn't write message to remote user.");
            }
        }

        private IPEndPoint GetRemoteEPFromMessage(ref string actualMessage)
        {
            int start = actualMessage.IndexOf('@');
            int end = actualMessage.IndexOf(' ');

            if (start == -1 || end == -1 || start > end || !actualMessage.StartsWith('@')) 
                throw new Exception();

            string remoteUser = actualMessage.Substring(start + 1, end - start + 1);
            actualMessage = actualMessage.Substring(end + 1);

            if (string.IsNullOrWhiteSpace(remoteUser) || string.IsNullOrWhiteSpace(actualMessage)) 
                throw new Exception();

            return _users_by_name[remoteUser];
        }

        private void SendToMulticastGroup(string message)
        {
            _buffer = Encoding.UTF8.GetBytes(message);
            _client.Send(_buffer, _buffer.Length, _multicast_group_ep);
        }
    }
}
