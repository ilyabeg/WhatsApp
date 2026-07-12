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
        private string _username = "user0";
        private readonly int _serverPortNumber = 13000;
        private readonly string _localhostIP = "127.0.0.1";

        public ClientTCP()
        {
            _client = new TcpClient(_localhostIP, _serverPortNumber);
            _stream = _client.GetStream();
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
            while (true)
            {
                Console.WriteLine("Enter message (or type 'X' to quit):");
                string message = Console.ReadLine();

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

        private void CloseProg()
        {
            Console.WriteLine("Exiting program...");
            _client.Close();
        }
    }
}
