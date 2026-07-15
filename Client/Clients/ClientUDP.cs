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
        private byte[] _buffer;
        private string _username = "user0";
        private readonly int _bufferSize = 4096;
        private readonly IPAddress _multicast_group_ip = IPAddress.Parse("239.1.1.1");
        private readonly int _listening_port = 20000;
        private readonly IPEndPoint _multicast_group_ep;

        public ClientUDP()
        {
            InitClient();

            _buffer = new byte[_bufferSize];
            _username = GetUserName(_username);     
            
            _multicast_group_ep = new IPEndPoint(_multicast_group_ip, _listening_port);
        }

        private void InitClient()
        {
            _client = new UdpClient();

            _client.Client.ExclusiveAddressUse = false;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _client.Client.Bind(new IPEndPoint(IPAddress.Any, _listening_port));
            _client.JoinMulticastGroup(_multicast_group_ip);
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
            Console.WriteLine("To Chat type: 'NEW' ...");
            Console.WriteLine("To Broadcast type: 'ALL' and write a message ...");
            Console.WriteLine("NOTE: Type 'CLEAR' to clear the screen at any time\n");

            Task.Run(Listen); // run listen task in the background     
            SendToMulticastGroup(_username + " is logged in...");

            while (true)
            {
                string message = Console.ReadLine().Trim();
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || !message.Equals("NEW", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine("Enter valid input.");
            else if (message.Equals("CLEAR", StringComparison.OrdinalIgnoreCase))
                Console.Clear();
            else
                Write(message);
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
            try
            {
                DataPacket recievedPacket = DataPacket.TransferData(recievedBytes);

                // if the message is meant for me -> print it, else, ignore it
                if (recievedPacket.Reciever.Equals(_username, StringComparison.OrdinalIgnoreCase) ||
                    recievedPacket.Reciever.Equals("all", StringComparison.OrdinalIgnoreCase))
                    PrintDataPacket(recievedPacket);
            }
            catch
            {
                PrintBytes(recievedBytes);
            }
        }

        private void PrintDataPacket(DataPacket recievedPacket)
        {
            Console.WriteLine($"({recievedPacket.Author}): {recievedPacket.Message}");
        }

        private void PrintBytes(byte[] recievedBytes)
        {
            string recievedString = Encoding.UTF8.GetString(recievedBytes);
            Console.WriteLine($"Recieved -> {recievedString}");
        }

        private void Write(string message)
        {
            try
            {
                DataPacket packet = DataPacket.CreateNew();
                packet.Author = _username;
               
                SendDataPacket(packet);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error! Couldn't write message to remote user due to {e.Message}");
            }
        }

        private void SendDataPacket(DataPacket packet)
        {
            string datapacket = JsonSerializer.Serialize(packet);
            SendToMulticastGroup(datapacket);
        }

        private void SendToMulticastGroup(string message)
        {
            _buffer = Encoding.UTF8.GetBytes(message);
            _client.Send(_buffer, _buffer.Length, _multicast_group_ep);
        }
    }
}
