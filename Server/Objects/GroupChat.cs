using Server.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Objects
{
    internal class GroupChat
    {
        public UdpClient GroupListener { get; set; }
        public string Name { get; set; }
        public int PortNumber { get; set; }
        public bool IsPrivate { get; set; } = false;

        public void RunGroup()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, PortNumber);

            try
            {
                while (true)
                {
                    byte[] receiveBytes = GroupListener.Receive(ref remoteEndPoint);
                    string message = Encoding.UTF8.GetString(receiveBytes);

                    GroupListener.Send(receiveBytes, receiveBytes.Length, groupEP);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error! GroupChat Crashed due to: {e.Message}");
                GroupListener.Close();
            }
        }
    }
}
