using Client.Clients;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.TCP
{
    internal class TcpClientHandler
    {
        private static readonly int _buffer_size = 4096;

        public static void ConnectAndSend(IPEndPoint remoteEP, string message, string author)
        {
            using TcpClient client = new TcpClient(); // make new client
            client.Connect(remoteEP); // connect to the remote user

            using NetworkStream stream = client.GetStream(); // open new stream

            byte[] buffer = Encoding.UTF8.GetBytes($"({author}): {message}");
            stream.Write(buffer, 0, buffer.Length);
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
    }
}
