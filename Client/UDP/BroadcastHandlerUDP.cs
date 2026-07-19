using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Client.UDP
{
    internal class BroadcastHandlerUDP
    {
        private static ConcurrentDictionary<string, Func<string[], Dictionary<string,  IPEndPoint>, int>> _options = new ConcurrentDictionary<string, Func<string[], Dictionary<string, IPEndPoint>, int>>()
        {
            ["$NEW_USER_SIGNAL$"] = (splitted, users) =>
            {
                string username = splitted[1];
                string endpoint = splitted[2];

                if (users.ContainsKey(username)) return -1; // <- returns a different option than 1 to not cause infinite loop
                AddNewUser(username, endpoint, users);                
                return 1;
            },
            ["$DISCONNECT_USER_SIGNAL$"] = (splitted, users) =>
            {
                string username = splitted[1];

                RemoveUser(username, users);
                return 2;
            },
            ["$ADD_GROUPS_SIGNAL$"] = (splitted, tmp) => // tmp is useless here but necessary to invoke the func
            {
                GroupChats.AddGroups(splitted[1]);
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
        public static int HandleBroadcast(byte[] recievedBytes, Dictionary<string, IPEndPoint> users)
        {
            string recieved = Encoding.UTF8.GetString(recievedBytes);
            string[] splitted = recieved.Split('#');
            string option = splitted[0];

            if (_options.ContainsKey(option))
                return _options[option].Invoke(splitted, users);

            // if handler doesn't recognise the broadcast return false
            return 0;
        }

        public static void AddNewUser(string username, string endpoint, Dictionary<string, IPEndPoint> users)
        {            
            if (!users.ContainsKey(username))
            {
                // split end point string (e.g. '127.0.0.1 : 5000')
                string[] splittedEndPoint = endpoint.Split(':');

                IPAddress ip = IPAddress.Parse(splittedEndPoint[0]); // <- 127.0.0.1
                int port = int.Parse(splittedEndPoint[1]);           // <- 5000

                IPEndPoint ep = new IPEndPoint(ip, port); // <- 127.0.0.1:5000

                Console.WriteLine($"[SYSTEM] User {username} logged in...");
                users.TryAdd(username, ep);
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
