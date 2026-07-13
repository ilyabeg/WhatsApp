using Client.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.Clients
{
    internal class ClientTCP : IClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private byte[] _buffer;
        private string _username = "user0";
        private readonly int _bufferSize = 4096;
        private readonly int _serverPortNumber = 13000;
        private readonly string _localhostIP = "127.0.0.1";

        public ClientTCP()
        {
            _client = new TcpClient(_localhostIP, _serverPortNumber);
            _stream = _client.GetStream();
            _buffer = new byte[_bufferSize];
            _username = GetUserName(_username);

            Send(_username); // send user name to let the server save it
        }

        private string GetUserName(string deafult)
        {
            Console.WriteLine($"Before starting to chat, enter your user name (current deafult: {deafult}):");
            string username = Console.ReadLine();

            if ( username != null && !username.IsWhiteSpace()) 
                return username;

            return deafult;
        }

        public void Start()
        {
            Console.WriteLine("To Start chatting type: '@user' and write a message (or type 'X' to quit):");
            Task.Run(() => Read()); // run read input task in the background 

            while (true)
            {
                string message = Console.ReadLine();

                if (message == null || message.IsWhiteSpace())
                {
                    Console.WriteLine("Please enter a valid input.");
                    continue;
                }

                if (message.ToLower() == "x") break;

                Send(message);
            }
            CloseProg();
        }

        private void Send(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            _stream.Write(buffer, 0, buffer.Length);
        }

        private void Read()
        {
            try
            {
                int totalRead;
                while ((totalRead = _stream.Read(_buffer, 0, _buffer.Length)) != 0)
                {
                    string recievedMessage = Encoding.UTF8.GetString(_buffer, 0, totalRead);
                    PrintMessageDetails(recievedMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error! Connection to server lost.");
            }
        }

        private void PrintMessageDetails(string message)
        {
            Console.WriteLine(message);
        }

        private void CloseProg()
        {
            Console.WriteLine("Exiting program...");
            _client.Close();
        }
    }
}
