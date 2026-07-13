using Server.Interfaces;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Servers
{
    internal class UdpServer : IServer
    {
        private UdpClient _listener;
        public static ConcurrentDictionary<string, UdpClient> _all_clients; // connected clients by their id
        private readonly int _listeningPortNumber = 14000;

        public UdpServer()
        {
            InitServer();
        }

        private void InitServer()
        {
            _all_clients = new ConcurrentDictionary<string, UdpClient>();
            _listener = new UdpClient(_listeningPortNumber);            
            Console.WriteLine("[SERVER] Server successfuly initialized.\n");
        }

        public void Run()
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            IPAddress clientIP = clientEndPoint.Address;
            int clientPort = clientEndPoint.Port;

            try
            {
                while (true)
                {
                    byte[] receiveBytes = _listener.Receive(ref clientEndPoint);
                    string message = Encoding.UTF8.GetString(receiveBytes);

                    Console.WriteLine($"[SERVER] Recieved: {message} from [{clientIP} : {clientPort}]");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error! Server Crashed due to: {e.Message}");
            }
        }
    }
}
