using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Client.TCP
{
    internal class BroadcastHandlerTCP
    {
        private ConcurrentDictionary<string, Func<string, IPEndPoint, Dictionary<string, IPEndPoint>, int>> _options = new ConcurrentDictionary<string, Func<string, IPEndPoint, Dictionary<string, IPEndPoint>, int>>()
        {
            ["$NEW_USER_SIGNAL$"] = (username, endpoint, users) =>
            {
                if (users.ContainsKey(username)) return -1; // <- returns a different option than 1 to not cause infinite loop

                users.TryAdd(username, endpoint);
                return 1;
            },
            ["$DISCONNECT_USER_SIGNAL$"] = (username, endpoint, users) =>
            {
                users.Remove(username);
                return 2;
            }
        };

        /// <summary>
        /// if handler knows how to handle the broadcast, handle and return the number of the option
        /// else, return 0 (couldn't hanlde)
        /// </summary>
        public int HandleBroadcast(byte[] recievedBytes, ref Dictionary<string, IPEndPoint> users)
        {
            string recieved = Encoding.UTF8.GetString(recievedBytes);
            string[] splitted = recieved.Split('#');

            if (splitted.Length < 2) return 0; // not a signal

            string option = splitted[0];
            string username = splitted[1];
            IPEndPoint? endpoint = null;

            if (option.Equals("$NEW_USER_SIGNAL$") && splitted.Length >= 3)
            {
                string[] splitted_endpoint = splitted[2].Split(':');
                IPAddress ip = IPAddress.Parse(splitted_endpoint[0]);
                int port = int.Parse(splitted_endpoint[1]);

                endpoint = new IPEndPoint(ip, port);
            }

            if (_options.ContainsKey(option))
                return _options[option].Invoke(splitted[1], endpoint, users);

            // if handler doesn't recognise the broadcast return false
            return 0;
        }
    }
}
