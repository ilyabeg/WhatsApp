using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

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
                IPAddress ip = GenerateIP(name);
                _groupChats.TryAdd(name, ip);
                client.JoinMulticastGroup(_groupChats[name]);
                Console.WriteLine($"[SYSTEM] Group {name} successfuly created on {ip}");
                Console.WriteLine("[SYSTEM] To chat in the group type: 'CHAT G' ...");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SYSTEM] Error! Couldn't add Group due to: {e.Message}");
            }
        }

        private static string InputGroupName()
        {
            Console.WriteLine("[SYSTEM] Enter Group Chat Name:");
            string name = Console.ReadLine().Trim();

            if (_groupChats.ContainsKey(name))
                throw new Exception($"Group Chat {name} already exists ...");

            return name;
        }

        private static IPAddress GenerateIP(string groupName)
        {
            using MD5 md5_hasher = MD5.Create();

            // get 16 byte array of the group name hash (128 bits)
            byte[] hash = md5_hasher.ComputeHash(Encoding.UTF8.GetBytes(groupName));

            // turn 2nd octet into the number 2 in case it is 1 or 0 to prevent collision with 
            // the ip addresses: 239.1.1.1 or 239.0.0.0
            byte octet2 = hash[0];
            if (octet2 <= 1)
                octet2 = 2;

            IPAddress ip = IPAddress.Parse($"{239}.{octet2}.{hash[1]}.{hash[2]}");
            return ip;
        }

        public static void SendToGroupChat(DataPacket packet, UdpClient client)
        {
            byte[] _buffer = new byte[_buffer_size];
            try
            {
                _buffer = Encoding.UTF8.GetBytes($"[{packet.Reciever}] -> ({packet.Author}): {packet.Message}");

                IPEndPoint endPoint = new IPEndPoint(_groupChats[packet.Reciever], _portNum);
                client.Send(_buffer, _buffer.Length, endPoint);
            }
            catch
            {
                Console.WriteLine($"[SYSTEM] Error! No Group chat {packet.Reciever} found...");
            }
        }

        public static void JoinGroup(UdpClient client)
        {
            DisplayGroups();
            if (_groupChats.Count > 0)
            {
                Console.WriteLine("[SYSTEM] Write the name of the group you'd like to join:");
                string name = Console.ReadLine().Trim();

                if (!_groupChats.ContainsKey(name))
                {
                    Console.WriteLine($"[SYSTEM] Error! No Group chat {name} found...");
                    return;
                }

                try
                {
                    client.JoinMulticastGroup(_groupChats[name]);
                    Console.WriteLine($"[SYSTEM] You have joined the group {name} successfuly.");
                    Console.WriteLine("[SYSTEM] To chat in the group type: 'CHAT G' ...");
                }
                catch
                {
                    Console.WriteLine($"[SYSTEM] Error! You are already a member of group {name}.");
                }
            }
        }

        public static void LeaveGroup(UdpClient client)
        {
            Console.WriteLine("[SYSTEM] Write the name of the group you'd like to leave:");
            string name = Console.ReadLine().Trim();

            if (!_groupChats.ContainsKey(name))
            {
                Console.WriteLine($"[SYSTEM] Error! No Group chat {name} found...");
                return;
            }

            client.DropMulticastGroup(_groupChats[name]);
            Console.WriteLine($"[SYSTEM] You have left the group {name} successfuly.");
        }

        private static void DisplayGroups()
        {
            Console.WriteLine("[SYSTEM] Available Groups:");

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
