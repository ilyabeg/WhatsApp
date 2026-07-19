using System.Collections.Concurrent;
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
        public static ConcurrentDictionary<string, IPAddress> groupChats { get; private set; } = new ConcurrentDictionary<string, IPAddress>();
        private static readonly int _buffer_size = 4096;

        public static (string groupName, IPAddress groupIP) CreateNewGroup(UdpClient client)
        {
            string name = null;
            IPAddress ip = null;

            try
            {
                name = InputGroupName();
                ip = GenerateIP(name);

                groupChats.TryAdd(name, ip);
                client.JoinMulticastGroup(groupChats[name]);

                Console.WriteLine($"[SYSTEM] Group {name} successfuly created on {ip}");
                Console.WriteLine("[SYSTEM] To chat in the group type: 'CHAT G' ...");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SYSTEM] Error! Couldn't add Group due to: {e.Message}");
            }
            return (name, ip);
        }

        private static string InputGroupName()
        {
            Console.WriteLine("[SYSTEM] Enter Group Chat Name:");
            string name = Console.ReadLine().Trim();

            if (groupChats.ContainsKey(name))
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

                IPEndPoint endPoint = new IPEndPoint(groupChats[packet.Reciever], _portNum);
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
            if (groupChats.Count > 0)
            {
                Console.WriteLine("[SYSTEM] Write the name of the group you'd like to join:");
                string name = Console.ReadLine().Trim();

                if (!groupChats.ContainsKey(name))
                {
                    Console.WriteLine($"[SYSTEM] Error! No Group chat {name} found...");
                    return;
                }

                try
                {
                    client.JoinMulticastGroup(groupChats[name]);
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

            if (!groupChats.ContainsKey(name))
            {
                Console.WriteLine($"[SYSTEM] Error! No Group chat {name} found...");
                return;
            }

            try
            {
                client.DropMulticastGroup(groupChats[name]);
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

            foreach (string groupname in groupChats.Keys)
            {
                Console.WriteLine($"\t- {groupname}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Return a serialized json string of the group chats 
        /// NOTE: turned the ip address to string aswell because of deserialization complications
        /// </summary>
        /// <returns></returns>
        public static string? GetGroups()
        {
            if (groupChats.Count > 0)
            {
                Dictionary<string, string> existing_groups = groupChats.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToString() // <- serialize the dictionary but with a string ip address
                );
                return JsonSerializer.Serialize(existing_groups);
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
            Dictionary<string, string>? existing_groups = JsonSerializer.Deserialize<Dictionary<string, string>>(str);

            foreach (string groupName in existing_groups.Keys)
            {
                if (!groupChats.ContainsKey(groupName))
                {
                    groupChats.TryAdd(groupName, IPAddress.Parse(existing_groups[groupName]));
                }
            }
        }
    }
}
