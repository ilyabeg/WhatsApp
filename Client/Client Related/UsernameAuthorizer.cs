using Client.Clients;
using System.Net.Sockets;

namespace Client.Client_Related
{
    internal class UsernameAuthorizer
    {
        public static volatile bool FreeUsername;

        /// <summary>
        /// Retuens true if the provided username is free and false if it is taken.
        /// </summary>
        public static bool IsFreeUsername(string username, UdpClient broadcaster)
        {
            while (true)
            {
                FreeUsername = true; // innocent until proven guilty

                if (string.IsNullOrWhiteSpace(username))
                {
                    return false;
                }

                // broadcast username to check if it is taken
                MulticastGroup.SendToMulticastGroup($"$CHECK_USERNAME_SIGNAL$#{username}", broadcaster);
                Thread.Sleep(250);

                if (!FreeUsername)
                {
                    return false; // username taken = NOT free
                }
                return true;
            }
        }
    }
}
