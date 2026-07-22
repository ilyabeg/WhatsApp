using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Client.UDP
{
    internal class GroupChats
    {
        private static readonly int _portNum = 20000;
        public static List<string> groupChats { get; private set; } = new List<string>();
        private static readonly int _buffer_size = 4096;

        private static readonly object _lock = new object();

        public static string CreateNewGroup(UdpClient client)
        {
            string? name = null;
            try
            {
                lock (_lock)
                {
                    name = InputGroupName();
                    groupChats.Add(name);
                }                
                client.JoinMulticastGroup(GenerateIP(name));

                Console.WriteLine($"[SYSTEM] Group {name} successfuly created");
                Console.WriteLine("[SYSTEM] To chat in the group type: 'CHAT G' ...");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SYSTEM] Error! Couldn't add Group due to: {e.Message}");
            }
            return name;
        }

        private static string InputGroupName()
        {
            Console.WriteLine("[SYSTEM] Enter Group Chat Name:");
            string name;

            if (groupChats.Contains(name))
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
            if (!groupChats.Contains(packet.Reciever))
            {
                Console.WriteLine($"[SYSTEM] Error! Group: {packet.Reciever} not found.");
                return;
            }

            try
            {   // try catch block in case user is not a member of the group
                client.JoinMulticastGroup(GenerateIP(packet.Reciever));
                Console.WriteLine($"[SYSTEM] You have joined the group {packet.Reciever} successfuly.");
            }
            catch { }

            try
            {
                byte[] _buffer = new byte[_buffer_size];
                _buffer = Encoding.UTF8.GetBytes($"[{packet.Reciever}] -> ({packet.Author}): {packet.Message}");

                IPEndPoint endPoint = new IPEndPoint(GenerateIP(packet.Reciever), _portNum);
                client.Send(_buffer, _buffer.Length, endPoint);
            }
            catch
            {
                Console.WriteLine($"[SYSTEM] Error! Couldn't Send Data Packet to: {packet.Reciever}.");
            }
        }

        public static void JoinGroup(UdpClient client)
        {
            DisplayGroups();
            if (groupChats.Count > 0)
            {
                Console.WriteLine("[SYSTEM] Write the name of the group you'd like to join:");
                string name;

                if (!groupChats.Contains(name))
                {
                    Console.WriteLine($"[SYSTEM] Error! No Group chat {name} found...");
                    return;
                }

                try
                {
                    client.JoinMulticastGroup(GenerateIP(name));
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

            if (!groupChats.Contains(name))
            {
                Console.WriteLine($"[SYSTEM] Error! No Group chat {name} found...");
                return;
            }

            try
            {
                client.DropMulticastGroup(GenerateIP(name));
                Console.WriteLine($"[SYSTEM] You have left the group {name} successfuly.");
            }
            catch
            {
                Console.WriteLine($"[SYSTEM] Error! You are not a member of group {name}.");

            }
        }        

        private static void DisplayGroups()
        {
            Console.WriteLine("[SYSTEM] Available Groups:");

            if (groupChats.Count == 0)
                Console.WriteLine("\t-None.\n");

            foreach (string groupname in groupChats)
            {
                Console.WriteLine($"\t- {groupname}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Return a serialized json string of the group chats 
        /// </summary>
        /// <returns></returns>
        public static string? GetGroups()
        {
            if (groupChats.Count > 0)
            {
                return JsonSerializer.Serialize(groupChats);
            }
            return null;
        }

        /// <summary>
        /// 
        /// Gets the existing groups and adds them localy to this client
        /// 
        /// </summary>
        /// <param name="str"></param>
        public static void AddGroups(string str)
        {
            List<string>? existing_groups = JsonSerializer.Deserialize<List<string>>(str);

            foreach (string groupName in existing_groups)
            {
                if (!groupChats.Contains(groupName))
                {
                    groupChats.Add(groupName);
                }
            }
        }
    }
}
