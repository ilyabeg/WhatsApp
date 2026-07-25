using Client.Client_Related;
using Client.Events;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Client.UDP
{
    internal class GroupChats
    {        
        public Dictionary<string, GroupChat> groupChats { get; private set; } = new Dictionary<string, GroupChat>();
        private readonly int _portNum = 20000;

        private readonly object _lock = new object();

        public event EventHandler<SystemErrorEventArgs> OnSystemError;
        public event EventHandler<GroupChangedEventArgs> OnGroupChange;

        public void CreateNewGroup(UdpClient client, string name)
        {
            try
            {               
                if (!groupChats.ContainsKey(name))
                {
                    lock (_lock)
                    {
                        GroupChat group = new GroupChat(name, true);
                        client.JoinMulticastGroup(GenerateIP(name));

                        group.MembersCount++;

                        groupChats.Add(name, group);
                        OnGroupChange?.Invoke(this, new GroupChangedEventArgs(groupChats.Values.ToList()));
                        return;
                    }
                }
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"GroupChat {name} already exists."));
            }
            catch
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"Couldn't create GroupChat {name}."));
            }
        }

        private IPAddress GenerateIP(string groupName)
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

        public void SendToGroup(UdpClient client, string username, string groupName, string message)
        {
            if (!groupChats.ContainsKey(groupName))
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"Group: {groupName} not found."));
                return;
            }

            try
            {   // try catch block in case user is not a member of the group
                client.JoinMulticastGroup(GenerateIP(groupName));
            }
            catch { }

            try
            {
                byte[] _buffer = Encoding.UTF8.GetBytes($"$GROUP_MESSAGE$#{groupName}#{username}#{message}");

                IPEndPoint endPoint = new IPEndPoint(GenerateIP(groupName), _portNum);
                client.Send(_buffer, _buffer.Length, endPoint);
            }
            catch
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"Couldn't Send message to: {groupName}."));
            }
        }

        public void JoinGroup(UdpClient client, string name)
        {
            if (!groupChats.ContainsKey(name))
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"No Group chat {name} found..."));
                return;
            }

            try
            {
                client.JoinMulticastGroup(GenerateIP(name));
                groupChats[name].MembersCount++;
                groupChats[name].IsMember = true;
                OnGroupChange?.Invoke(this, new GroupChangedEventArgs(groupChats.Values.ToList()));
            }
            catch
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"You are already a member of Group: {name}."));
            }
        }

        public void LeaveGroup(UdpClient client, string name)
        {
            if (!groupChats.ContainsKey(name))
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"No GroupChat {name} found."));
                return;
            }

            try
            {
                client.DropMulticastGroup(GenerateIP(name));
                groupChats[name].MembersCount--;
                groupChats[name].IsMember = false;

                if (groupChats[name].MembersCount == 0)
                {
                    lock (_lock)
                    {
                        groupChats.Remove(name);
                    }                    
                }
                OnGroupChange?.Invoke(this, new GroupChangedEventArgs(groupChats.Values.ToList()));
            }
            catch
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"You are not a member of Group: {name}."));
            }
        }        

        // <=== Broadcasting services ===>

        /// <summary>
        /// Return a serialized json string of the group chats 
        /// </summary>
        /// <returns></returns>
        public string? GetGroups()
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
        public void AddGroups(string str)
        {
            Dictionary<string, GroupChat>? existing_groups = JsonSerializer.Deserialize<Dictionary<string, GroupChat>>(str);
            if (existing_groups == null) return;

            bool groupsUpdated = false;
            foreach (string groupName in existing_groups.Keys)
            {
                if (!groupChats.ContainsKey(groupName))
                {
                    groupChats.Add(groupName, existing_groups[groupName]);
                    groupsUpdated = true;
                }
                else
                {
                    if (groupChats[groupName].MembersCount != existing_groups[groupName].MembersCount)
                    {
                        groupChats[groupName].MembersCount = existing_groups[groupName].MembersCount;
                        groupsUpdated = true;
                    }
                }
            }

            if (groupsUpdated)
            {
                OnGroupChange?.Invoke(this, new GroupChangedEventArgs(groupChats.Values.ToList()));
            }
        }
    }
}
