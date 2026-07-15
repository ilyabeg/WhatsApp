using Client.Application;
using Client.Interfaces;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
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

        private Dictionary<string, Action> _input_option;

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

            _input_option = new Dictionary<string, Action>();
            InitOptions();
        }

        private void InitOptions()
        {
            _input_option.Add("CHAT", () => {
                Console.WriteLine("To broadcast specify the destination as 'ALL' ...");
                DataPacket packet = Write();
                SendDataPacket(packet);
            });
            _input_option.Add("CHAT G", () => {
                DataPacket packet = Write();
                GroupChats.SendToGroupChat(packet, _client);
            });
            _input_option.Add("NEW G", () => GroupChats.CreateNewGroup(_client));
            _input_option.Add("JOIN G", () => GroupChats.JoinGroup(_client));
            _input_option.Add("LEAVE G", () => GroupChats.LeaveGroup(_client));
            _input_option.Add("OPTIONS", DisplayOptions);
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
            DisplayOptions();

            Task.Run(Listen); // run listen task in the background     
            SendToMulticastGroup(_username + " is logged in...");

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
                Console.WriteLine("Enter valid input.");
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
                Console.WriteLine($"Error! Couldn't write message to remote user due to {e.Message}");
            }
            return null;
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

        private void DisplayOptions()
        {
            Console.WriteLine("To Chat type: 'CHAT' ...");
            Console.WriteLine("To Create a new Group type: 'NEW G' ...");
            Console.WriteLine("To Join a Group type: 'JOIN G' ...");
            Console.WriteLine("To Leave a Group type: 'LEAVE G' ...");
            Console.WriteLine("To Display Options type: 'OPTIONS' ...");
            Console.WriteLine("NOTE: Type 'CLEAR' to clear the screen at any time\n");
        }
    }
}
