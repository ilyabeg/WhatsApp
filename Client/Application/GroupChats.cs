using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

namespace Client.Application
{
    internal class GroupChats
    {
        private static readonly int _portNum = 20000;
        public static ConcurrentDictionary<string, IPAddress> _groupChats { get; private set; } = new ConcurrentDictionary<string, IPAddress>();
        private static readonly int _buffer_size = 4096;

        public static void CreateNewGroup(UdpClient client)
        {
            try
            {
                string name = InputGroupName();
                IPAddress ip = InputIP();
                _groupChats.TryAdd(name, ip);
                client.JoinMulticastGroup(_groupChats[name]);
                Console.WriteLine($"Group {name} successfuly created on {ip}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error! Couldn't add Group due to: {e.Message}");
            }
        }

        private static string InputGroupName()
        {
            Console.WriteLine("Enter Group Chat Name:");
            string name = Console.ReadLine().Trim();

            if (_groupChats.ContainsKey(name))
                throw new Exception($"Group Chat {name} already exists ...");

            return name;
        }

        private static IPAddress InputIP()
        {
            Console.WriteLine("Enter Group Chat IP:");
            IPAddress ipAddress = IPAddress.Parse(Console.ReadLine().Trim());

            foreach (IPAddress ip in _groupChats.Values)
            {
                if (ip.Equals(ipAddress))
                    throw new Exception($"IP {ipAddress} already exists ...");
            }            
            return ipAddress;
        }

        public static void SendToGroupChat(DataPacket packet, UdpClient client)
        {
            byte[] _buffer = new byte[_buffer_size];
            try
            {
                _buffer = Encoding.UTF8.GetBytes($"[{_groupChats[packet.Reciever]}] -> ({packet.Author}): {packet.Message}");

                IPEndPoint endPoint = new IPEndPoint(_groupChats[packet.Reciever], _portNum);
                client.Send(_buffer, _buffer.Length, endPoint);
            }
            catch
            {
                Console.WriteLine($"Error! No Group chat {packet.Reciever} found...");
            }
        }

        public static void JoinGroup(UdpClient client)
        {
            DisplayGroups();
            if (_groupChats.Count > 0)
            {
                Console.WriteLine("Write the name of the group you'd like to join:");
                string name = Console.ReadLine().Trim();

                if (!_groupChats.ContainsKey(name))
                {
                    Console.WriteLine($"Error! No Group chat {name} found...");
                    return;
                }

                client.JoinMulticastGroup(_groupChats[name]);
                Console.WriteLine($"You have joined the group {name} successfuly.");
                Console.WriteLine("To chat in the group type: 'CHAT G' ...");
            }            
        }

        public static void LeaveGroup(UdpClient client)
        {
            Console.WriteLine("Write the name of the group you'd like to leave:");
            string name = Console.ReadLine().Trim();

            if (!_groupChats.ContainsKey(name))
            {
                Console.WriteLine($"Error! No Group chat {name} found...");
                return;
            }

            client.DropMulticastGroup(_groupChats[name]);
            Console.WriteLine($"You have left the group {name} successfuly.");
        }

        private static void DisplayGroups()
        {
            Console.WriteLine("Available Groups:");

            if (_groupChats.Count == 0)
                Console.WriteLine("\t-None.\n");

            foreach (string groupname in _groupChats.Keys)
            {
                Console.WriteLine($"\t- {groupname}");
            }
            Console.WriteLine();
        }
    }
}
