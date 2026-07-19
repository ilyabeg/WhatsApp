using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Client.UDP
{
    internal class BroadcastHandlerUDP
    {
        private static ConcurrentDictionary<string, Func<string, IPEndPoint, Dictionary<string,  IPEndPoint>, int>> _options = new ConcurrentDictionary<string, Func<string, IPEndPoint, Dictionary<string, IPEndPoint>, int>>()
        {
            ["$NEW_USER_SIGNAL$"] = (username, endpoint, users) =>
            {
                if (users.ContainsKey(username)) return -1; // <- returns a different option than 1 to not cause infinite loop
                AddNewUser(username, endpoint, users);                
                return 1;
            },
            ["$DISCONNECT_USER_SIGNAL$"] = (username, endpoint, users) => // endpoint is useless here but necessary to invoke the func
            {
                RemoveUser(username, users);
                return 2;
            },
            ["$ADD_GROUPS_SIGNAL$"] = (existingGroups, tmp1, tmp2) => // temps are useless here but necessary to invoke the func
            {
                GroupChats.AddGroups(existingGroups);
                return 3;
            }
        };

        /// <summary>
        /// if handler knows how to handle the broadcast, handle and return the number of the option
        /// else, return 0 (couldn't hanlde)
        /// </summary>
        /// <param name="recievedBytes"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        public static int HandleBroadcast(byte[] recievedBytes, IPEndPoint remoteEndPoint, Dictionary<string, IPEndPoint> users)
        {
            string recieved = Encoding.UTF8.GetString(recievedBytes);
            string[] splitted = recieved.Split('#');
            string option = splitted[0];

            if (_options.ContainsKey(option))
                return _options[option].Invoke(splitted[1], remoteEndPoint, users);

            // if handler doesn't recognise the broadcast return false
            return 0;
        }

        public static void AddNewUser(string username, IPEndPoint endpoint, Dictionary<string, IPEndPoint> users)
        {            
            if (!users.ContainsKey(username))
            {
                Console.WriteLine($"[SYSTEM] User {username} logged in...");
                users.TryAdd(username, endpoint);
            }
        }

        public static void RemoveUser(string username, Dictionary<string, IPEndPoint> users)
        {
            if (users.ContainsKey(username))
            {
                Console.WriteLine($"[SYSTEM] User {username} Disconnected.");
                users.Remove(username);
            }
        }
    }
}
