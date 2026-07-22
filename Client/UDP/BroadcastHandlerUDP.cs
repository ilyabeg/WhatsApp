using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Client.UDP
{
    internal class BroadcastHandlerUDP
    {
        private ConcurrentDictionary<string, Func<string, IPEndPoint, Dictionary<string,  IPEndPoint>, GroupChats, int>> _options = new()
        {
            ["$NEW_USER_SIGNAL$"] = (username, endpoint, users, groups) =>
            {
                if (users.ContainsKey(username)) 
                    return -1; // <- returns a different option than 1 to not cause infinite loop

                users.TryAdd(username, endpoint);
                return 1;
            },
            ["$DISCONNECT_USER_SIGNAL$"] = (username, endpoint, users, groups) => // endpoint is useless here but necessary to invoke the func
            {
                users.Remove(username);
                return 2;
            },
            ["$ADD_GROUPS_SIGNAL$"] = (existingGroups, endpoint, user, groups) => // temps are useless here but necessary to invoke the func
            {
                groups.AddGroups(existingGroups);
                return 3;
            }
        };

        /// <summary>
        /// if handler knows how to handle the broadcast, handle and return the number of the option
        /// else, return 0 (couldn't hanlde)
        /// </summary>
        public int HandleBroadcast(byte[] recievedBytes, IPEndPoint remoteEndPoint, Dictionary<string, IPEndPoint> users, GroupChats groups)
        {
            string recieved = Encoding.UTF8.GetString(recievedBytes);
            string[] splitted = recieved.Split('#');
            string option = splitted[0];

            if (_options.ContainsKey(option))
                return _options[option].Invoke(splitted[1], remoteEndPoint, users, groups);

            // if handler doesn't recognise the broadcast return false
            return 0;
        }
    }
}
