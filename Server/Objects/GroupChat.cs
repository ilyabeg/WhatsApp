using System.Net.Sockets;

namespace Server.Objects
{
    internal class GroupChat
    {
        public UdpClient GroupListener { get; set; }
        public string Name { get; set; }
        public int PortNumber { get; set; }
        public bool IsPrivate { get; set; } = false;
    }
}
