using Server.Handlers;
using Server.InputHandlers;
using Server.Interfaces;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Servers
{
    internal class TcpServer : IServer
    {
        private TcpListener _listener;
        public static ConcurrentDictionary<string, TcpClient> _all_clients; // connected clients by their id
        private readonly int _listeningPortNumber = 13000;
        private readonly IPAddress _localhostIP = IPAddress.Parse("127.0.0.1");   
        
        private TcpIOHandler _ioHandler;
        private TcpClientHandler _clientHandler;

        public TcpServer()
        {
            StartServer();
        }

        private void StartServer()
        {
            _listener = new TcpListener(_localhostIP, _listeningPortNumber);
            _all_clients = new ConcurrentDictionary<string, TcpClient>();
            _ioHandler = new TcpIOHandler();
            _clientHandler = new TcpClientHandler();

            Console.WriteLine("[SERVER] Server successfuly initialized.\n");
            _listener.Start();
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    // start independent task for each client
                    TcpClient client = _listener.AcceptTcpClient();
                    Task.Run(() => ListenToClient(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER ERROR!] Server crashed due to: {ex.Message}");
            }
        }

        private void ListenToClient(TcpClient client)
        {
            string clientID = _clientHandler.GetClientID(ref client); // first input from client (username)
            _clientHandler.AddClient(clientID, client);
            _ioHandler.DisplayConnectedClients(client);

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];

            try
            {                
                int totalRead;
                while ((totalRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string recievedMessage = Encoding.UTF8.GetString(buffer, 0, totalRead);
                    _ioHandler.PrintMessageDetails(recievedMessage, clientID); // echo message

                    _ioHandler.HandleMessage(recievedMessage, client, clientID);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("[SERVER ERROR!] Client Disconnected.");
                _all_clients.Remove(clientID, out _);
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER ERROR!] Server crashed due to: {ex.Message}");
            }
        }               
    }
}
