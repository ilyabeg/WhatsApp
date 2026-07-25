using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.Clients
{
    internal class MulticastGroup
    {
        public static readonly int UdpPort = 20000;
        public static readonly int TcpPort = 21000;

        public static readonly IPAddress IPAddress = IPAddress.Parse("239.1.1.1");

        public static void AddToMulticastGroup(UdpClient client)
        {
            client.JoinMulticastGroup(IPAddress);
        }

        public static void SendToMulticastGroup(string message, UdpClient client, int portNum)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            client.Send(buffer, buffer.Length, new IPEndPoint(IPAddress, portNum));
        }
    }
}
