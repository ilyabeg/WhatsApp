using Client.Events;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Client.TCP
{
    internal class TcpClientHandler
    {
        public event EventHandler<SystemErrorEventArgs> OnSystemError;
        public event EventHandler<MessageRecievedEventArgs> OnMessageReceived;

        private static readonly int _buffer_size = 4096;
        private ConcurrentDictionary<string, TcpClient> _open_connections = new ConcurrentDictionary<string, TcpClient>();

        public void ConnectAndSend(string remoteUsername, IPEndPoint remoteEP, string message, string sender)
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

                // send message using json text
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "Sender", sender },
                    { "Message", message }
                };
                string jsonData = JsonSerializer.Serialize(data);

                byte[] buffer = Encoding.UTF8.GetBytes(jsonData);
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                // if writing to the stream failed, dispose the client.                
                if (_open_connections.TryRemove(remoteUsername, out TcpClient client))
                {
                    client.Dispose();
                }

                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"Couldn't write to remote client due to: {e.Message}"));
            }
        }

        public void HandleRemoteClient(TcpClient client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[_buffer_size];

                int totalRead;
                while ((totalRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string recievedMessage = Encoding.UTF8.GetString(buffer, 0, totalRead);

                    try
                    {
                        var received_data = JsonSerializer.Deserialize<Dictionary<string, string>>(recievedMessage);

                        if (received_data != null && received_data.ContainsKey("Sender") && received_data.ContainsKey("Message"))
                        {
                            OnMessageReceived?.Invoke(this, new MessageRecievedEventArgs(received_data["Sender"], received_data["Message"]));
                        }
                    }
                    catch
                    { }
                }

                client.Dispose(); // dispose client when done
            }
            catch (Exception e)
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"Connection to remote client lost due to: {e.Message}"));
            }
        }

        public void DisposeConnections()
        {
            foreach (TcpClient client in _open_connections.Values)
            {
                client?.Dispose();
            }
        }
    }
}
