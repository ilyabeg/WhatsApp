using Client.Interfaces;
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
        private readonly int _serverPortNumber = 14000;
        private readonly string _localhostIP = "127.0.0.1";

        public ClientUDP()
        {
            _client = new UdpClient();
            _buffer = new byte[_bufferSize];
            _username = GetUserName(_username);

            Send(_username); // send over username to the server
        }

        private string GetUserName(string deafult)
        {
            Console.WriteLine($"Before starting to chat, enter your user name (current deafult: {deafult}):");
            string username = Console.ReadLine();

            if (username != null && !username.IsWhiteSpace())
                return username;

            return deafult;
        }

        public void Start()
        {
            Console.WriteLine("NOTE: Type 'CLEAR' to clear the screen at any time");
            Task.Run(Read); // run read input task in the background 

            while (true)
            {
                string message = Console.ReadLine().Trim();

                if (message.Equals("CLEAR", StringComparison.OrdinalIgnoreCase)) 
                    Console.Clear();
                else
                    Send(message);
            }            
        }

        public void Read()
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // any remote user/server 

                while (true)
                {
                    byte[] recievedBytes = _client.Receive(ref remoteEndPoint);
                    string recievedMessage = Encoding.UTF8.GetString(recievedBytes);

                    PrintMessage(recievedMessage);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error! Connection to server lost.");
            }
        }

        private void PrintMessage(string message)
        {
            Console.WriteLine(message);
        }

        private void Send(string message)
        {
            _buffer = Encoding.UTF8.GetBytes(message);
            _client.Send(_buffer, _buffer.Length, _localhostIP, _serverPortNumber);
        }
    }
}
