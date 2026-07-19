using Client.Clients;
using Client.Interfaces;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.TCP
{
    internal class TcpClientHandler
    {
        private static readonly int _buffer_size = 4096;
        private static ConcurrentDictionary<string, TcpClient> _open_connections = new ConcurrentDictionary<string, TcpClient>();

        public static void ConnectAndSend(string remoteUsername, IPEndPoint remoteEP, string message, string author)
        {
            try
            {
                TcpClient remote_client;
                if (_open_connections.ContainsKey(remoteUsername))
                {
                    remote_client = _open_connections[remoteUsername];
                }
                else
                {
                    remote_client = new TcpClient(); // make new client
                    remote_client.Connect(remoteEP); // connect to the remote user
                    _open_connections.TryAdd(remoteUsername, remote_client);
                }

                NetworkStream stream = remote_client.GetStream();

                byte[] buffer = Encoding.UTF8.GetBytes($"({author}): {message}");
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                // if writing to the stream failed, dispose the client.
                Console.WriteLine($"[SYSTEM] Error! Couldn't write to client due to: {e.Message}");
                if (_open_connections.TryRemove(remoteUsername, out TcpClient client))
                {
                    client.Dispose();
                }
            }
        }

        public static void HandleRemoteClient(TcpClient client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[_buffer_size];

                int totalRead;
                while ((totalRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string recievedMessage = Encoding.UTF8.GetString(buffer, 0, totalRead);
                    Printer.PrintMessage(recievedMessage);
                }

                client.Dispose(); // dispose client when done
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error! Connection to remote user lost due to: {e.Message}");
            }
        }

        public static void DisposeConnections()
        {
            foreach (TcpClient client in _open_connections.Values)
            {
                client?.Dispose();
            }
        }
    }
}
