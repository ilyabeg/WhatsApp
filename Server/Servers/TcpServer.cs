using Server.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Servers
{
    internal class TcpServer : IServer
    {
        private TcpListener _listener;
        private readonly int _listeningPortNumber = 13000;
        private readonly IPAddress _localhostIP = IPAddress.Parse("127.0.0.1");

        public TcpServer()
        {
            StartServer();
        }

        public void Listen()
        {            
            using TcpClient client = _listener.AcceptTcpClient();
            Stream stream = client.GetStream();

            byte[] buffer = new byte[client.ReceiveBufferSize];                        

            int totalRead;
            while ((totalRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string recievedMessage = Encoding.UTF8.GetString(buffer, 0, totalRead);
                Console.WriteLine($"[SERVER] Recieved message: {recievedMessage}.");
            }
        }

        private void StartServer()
        {            
            _listener = new TcpListener(_localhostIP, _listeningPortNumber);
            _listener.Start();
        }
    }
}
