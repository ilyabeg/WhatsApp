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
        private ConcurrentDictionary<string, TcpClient> _all_clients; // connected client by their id
        private readonly int _listeningPortNumber = 13000;
        private readonly IPAddress _localhostIP = IPAddress.Parse("127.0.0.1");        

        public TcpServer()
        {
            StartServer();
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
                Console.WriteLine($"[SERVER ERROR!] {ex.Message}");
            }
        }

        private void ListenToClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                string clientID = GetClientID(client, stream, buffer); // first input from client (username)
                DisplayConnectedClients(client, stream);

                int totalRead;
                while ((totalRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string recievedMessage = Encoding.UTF8.GetString(buffer, 0, totalRead);
                    PrintMessageDetails(client, recievedMessage, clientID);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("[SERVER ERROR!] Client Disconnected Forcefuly.");
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER ERROR!] {ex.Message}");
            }
        }

        private void PrintMessageDetails(TcpClient client, string msg, string clientID)
        {
            IPEndPoint endPoint = client.Client.RemoteEndPoint as IPEndPoint;
            string clientIP = endPoint.Address.ToString();
            int clientPort = endPoint.Port;

            Console.WriteLine($"[SERVER] Recieved message: '{msg}' from {clientID} [{clientIP} : {clientPort}].");
        }

        private void DisplayConnectedClients(TcpClient client, NetworkStream clientStream)
        {
            string output = "[SERVER] Connected users:";
            foreach (string username in _all_clients.Keys)
            {
                output += "\n\t- " + username;
            }
            byte[] buffer = Encoding.UTF8.GetBytes("\n" + output);
            clientStream.Write(buffer, 0, buffer.Length);
        }

        private string GetClientID(TcpClient client, NetworkStream stream, byte[] buffer)
        {
            int totalRead = stream.Read(buffer, 0, buffer.Length);
            string username = Encoding.UTF8.GetString(buffer, 0, totalRead);
            AddClient(username, client);

            return username;
        }

        private void AddClient(string username, TcpClient client)
        {
            if (_all_clients.ContainsKey(username))
            {
                // attach last 4 numbers of the hash to make username almost always unique 
                string hash = username.GetHashCode().ToString();
                username += hash.Substring(hash.Length - 4);
            }
            _all_clients.TryAdd(username, client);
            Console.WriteLine($"\n[SERVER] New User {username} logged in...\n");
        }

        private void StartServer()
        {            
            _listener = new TcpListener(_localhostIP, _listeningPortNumber);
            _all_clients = new ConcurrentDictionary<string, TcpClient>();
            Console.WriteLine("[SERVER] Server successfuly initialized.\n");
            _listener.Start();
        }
    }
}
