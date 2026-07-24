using Client.Events;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Client.UDP
{
    internal class BroadcastHandlerUDP
    {
        private ConcurrentDictionary<string, Func<string, IPEndPoint, Dictionary<string, IPEndPoint>, GroupChats, int>> _options = new()
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
            },
            ["$GROUP_MESSAGE$"] = (existingGroups, endpoint, user, groups) => 4 // return signal code 4
        };

        // define public events to bubble over to the Client
        public event EventHandler<MessageRecievedEventArgs> OnMessageReceived;
        public event EventHandler<UserChangedEventArgs> OnUserChanged;

        /// <summary>
        /// if handler knows how to handle the broadcast, handle and return the number of the option
        /// else, return 0 (couldn't hanlde)
        /// </summary>
        public int HandleBroadcast(byte[] recievedBytes, IPEndPoint remoteEndPoint, Dictionary<string, IPEndPoint> users, GroupChats groups)
        {
            string recieved = Encoding.UTF8.GetString(recievedBytes);
            string[] splitted = recieved.Split('#');

            if (splitted.Length < 2) return 0; // not signal

            string option = splitted[0];

            int executed_option = 0;
            if (_options.ContainsKey(option))
                executed_option = _options[option].Invoke(splitted[1], remoteEndPoint, users, groups);

            CheckExecutedOption(executed_option, recieved);

            return executed_option;
        }

        private void CheckExecutedOption(int executed_option, string received_string)
        {
            string[] splitted = received_string.Split('#');

            // if handler doesn't recognise the broadcast signal the process it as a simple broadcast and return 0
            if (executed_option == 0)
            {
                // has to be simple broadcast message for example: $"{username}#{message}"
                string sender = splitted[0];
                string message = splitted[1];

                OnMessageReceived?.Invoke(this, new MessageRecievedEventArgs(sender, message));
            }

            // if we removed the user invoke user changed event
            else if (executed_option == 2)
                // notify UI the users list
                OnUserChanged?.Invoke(this, new UserChangedEventArgs(splitted[1], State.Disconnecting));

            // if group message received invoke OnMessageReceived from the GroupChat name
            if (executed_option == 4)
            {
                string groupName = splitted[1];
                string sender = splitted[2];
                string message = splitted[3];

                OnMessageReceived?.Invoke(this, new MessageRecievedEventArgs(groupName, $"{sender}: {message}"));
            }
        }
    }
}
