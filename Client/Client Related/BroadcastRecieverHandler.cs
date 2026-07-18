using Client.UDP;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Client.Client_Related
{
    internal class BroadcastRecieverHandler
    {
        private static readonly int _port = 20000;
        private static readonly IPEndPoint _localEndPoint = new IPEndPoint(IPAddress.Loopback, _port);

        private static ConcurrentDictionary<string, Func<string, Dictionary<string, IPEndPoint>, int>> _options = new ConcurrentDictionary<string, Func<string, Dictionary<string, IPEndPoint>, int>>()
        {
            ["$NEW_USER_SIGNAL$"] = (username, users) =>
            {
                AddNewUser(username, ref users);                
                return 1;
            },
            ["$DISCONNECT_USER_SIGNAL$"] = (username, users) =>
            {
                RemoveUser(username, ref users);
                return 2;
            }
        };

        /// <summary>
        /// if handler knows how to handle the broadcast, handle and return the number of the potion
        /// else, return 0 (couldn't hanlde)
        /// </summary>
        /// <param name="recievedBytes"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        public static int HandleBroadcast(byte[] recievedBytes, ref Dictionary<string, IPEndPoint> users)
        {
            string recieved = Encoding.UTF8.GetString(recievedBytes);
            string[] splitted = recieved.Split('#');
            string option = splitted[0];

            if (_options.ContainsKey(option))
                return _options[option].Invoke(splitted[1], users);

            // if handler doesn't recognise the broadcast return false
            return 0;
        }

        public static void AddNewUser(string username, ref Dictionary<string, IPEndPoint> users)
        {
            if (!users.ContainsKey(username))
            {               
                Console.WriteLine($"[SYSTEM] New User {username} logged in...");
                users.TryAdd(username, _localEndPoint);
            }
        }

        public static void RemoveUser(string username, ref Dictionary<string, IPEndPoint> users)
        {
            if (users.ContainsKey(username))
            {
                Console.WriteLine($"[SYSTEM] User {username} Disconnected.");
                users.Remove(username);
            }
        }

        //public static void AddNewGroup(string name, string endpoint)
        //{
        //    if (!users.ContainsKey(username))
        //    {
        //        string[] splitedEndPoint = endpoint.Split(':');

        //        IPAddress ip = IPAddress.Parse(splitedEndPoint[0]);
        //        int port = int.Parse(splitedEndPoint[1]);

        //        IPEndPoint ep = new IPEndPoint(ip, port);

        //        Console.WriteLine($"[SYSTEM] New User {username} logged in...");
        //        users.TryAdd(username, ep);
        //    }
        //}
    }
}
