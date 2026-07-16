using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.Application
{
    internal class MulticastGroup
    {
        public static readonly int Port = 20000;
        public static readonly IPAddress IPAddress = IPAddress.Parse("239.1.1.1");
        public static readonly IPEndPoint EndPoint = new IPEndPoint(IPAddress, Port);

        private static int _buffer_size = 4096;
        private static byte[] _buffer = new byte[_buffer_size];

        public static void AddToMulticastGroup(UdpClient client)
        {
            client.JoinMulticastGroup(IPAddress);
        }

        public static void SendToMulticastGroup(string message, UdpClient client)
        {
            _buffer = Encoding.UTF8.GetBytes(message);
            client.Send(_buffer, _buffer.Length, EndPoint);
        }
    }
}
