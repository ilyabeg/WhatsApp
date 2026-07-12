using Client.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.Clients
{
    internal class ClientTCP : IClient
    {
        private TcpClient _client;
        private readonly int _serverPortNumber = 13000;
        private readonly string _localhostIP = "127.0.0.1";

        public ClientTCP()
        {
            _client = new TcpClient(_localhostIP, _serverPortNumber);
        }

        public void Start()
        {
            NetworkStream stream = _client.GetStream();

            while (true)
            {
                Console.WriteLine("Enter message (or type 'exit' to quit):");
                string message = Console.ReadLine();

                if (message.ToLower() == "exit") break;

                byte[] buffer = Encoding.UTF8.GetBytes(message);

                stream.Write(buffer, 0, buffer.Length);
            }
            Console.WriteLine("Exiting program...");
            _client.Close();
        }
    }
}
