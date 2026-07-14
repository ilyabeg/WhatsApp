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
        public IPEndPoint EndPoint { get; set; }
        public bool IsPrivate { get; set; } = false;

        public List<IPEndPoint> members { get; private set; }  = new List<IPEndPoint>();

        public void RunGroup()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);               
            try
            {
                while (true)
                {
                    byte[] receiveBytes = GroupListener.Receive(ref remoteEndPoint);
                    string message = Encoding.UTF8.GetString(receiveBytes);

                    message = $"[From: {Name}] - " + message;
                    byte[] sendBytes = Encoding.UTF8.GetBytes(message);

                    // send evry incoming message to all group members
                    foreach (IPEndPoint memberEP in members)
                    {
                        GroupListener.Send(sendBytes, sendBytes.Length, memberEP);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error! GroupChat {Name} Crashed due to: {e.Message}");
                GroupListener.Close();
            }
        }

        public void Start()
        {
            Task.Run(RunGroup);
        }

        public void AddMember(IPEndPoint clientEP)
        {
            if (!members.Contains(clientEP))
            {
                members.Add(clientEP);
                Console.WriteLine($"Member {clientEP} successfuly added to group {Name}.");
            }
        }
    }
}
