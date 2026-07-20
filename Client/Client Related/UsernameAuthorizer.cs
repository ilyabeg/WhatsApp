using Client.Clients;
using System.Net.Sockets;

namespace Client.Client_Related
{
    internal class UsernameAuthorizer
    {
        public static volatile bool FreeUsername;
        public static string GetUsername(UdpClient udpClient)
        {
            Console.WriteLine("[SYSTEM] Before starting to chat, enter your user name:");
            while (true)
            {
                string username = Console.ReadLine().Trim();
                FreeUsername = true; // innocent until proven guilty

                if (string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine("[SYSTEM] Please enter valid username:");
                    continue;
                }
                // broadcast username to check if it is taken
                MulticastGroup.SendToMulticastGroup($"$CHECK_USERNAME_SIGNAL$#{username}", udpClient);
                Thread.Sleep(250);

                if (!FreeUsername)
                {
                    Console.WriteLine("[SYSTEM] Username already taken. Please re-enter:");
                    continue;
                }
                return username;
            }
        }
    }
}
