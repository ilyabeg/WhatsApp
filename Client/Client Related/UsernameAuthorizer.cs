using Client.Clients;
using System.Net.Sockets;

namespace Client.Client_Related
{
    internal class UsernameAuthorizer
    {       
        public static string GetUsername()
        {
            Console.WriteLine("[SYSTEM] Before starting to chat, enter your user name:");
            while (true)
            {
                string username = Console.ReadLine().Trim();

                if (string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine("[SYSTEM] Please enter valid username:");
                    continue;
                }

                return username;
            }
        }
    }
}
