using System.Collections.Concurrent;
using System.Text;

namespace Client.UDP
{
    internal class BroadcastHandlerUDP
    {
        private static ConcurrentDictionary<string, Func<string, List<string>, int>> _options = new ConcurrentDictionary<string, Func<string, List<string>, int>>()
        {
            ["$NEW_USER_SIGNAL$"] = (username, users) =>
            {
                if (users.Contains(username)) return -1; // <- returns a different option than 1 to not cause infinite loop
                AddNewUser(username, ref users);                
                return 1;
            },
            ["$DISCONNECT_USER_SIGNAL$"] = (username, users) =>
            {
                RemoveUser(username, ref users);
                return 2;
            },
            ["$ADD_GROUPS_SIGNAL$"] = (groups_string, tmp) => // tmp is useless here but necessary to invoke the func
            {
                GroupChats.AddGroups(groups_string);
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
        public static int HandleBroadcast(byte[] recievedBytes, ref List<string> users)
        {
            string recieved = Encoding.UTF8.GetString(recievedBytes);
            string[] splitted = recieved.Split('#');
            string option = splitted[0];

            if (_options.ContainsKey(option))
                return _options[option].Invoke(splitted[1], users);

            // if handler doesn't recognise the broadcast return false
            return 0;
        }

        public static void AddNewUser(string username, ref List<string> users)
        {
            if (!users.Contains(username))
            {               
                Console.WriteLine($"[SYSTEM] User {username} logged in...");
                users.Add(username);
            }
        }

        public static void RemoveUser(string username, ref List<string> users)
        {
            if (users.Contains(username))
            {
                Console.WriteLine($"[SYSTEM] User {username} Disconnected.");
                users.Remove(username);
            }
        }
    }
}
