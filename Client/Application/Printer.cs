using System.Net;
using System.Text;

namespace Client.Application
{
    internal class Printer
    {
        public static void PrintDataPacket(DataPacket recievedPacket)
        {
            Console.WriteLine($"({recievedPacket.Author}): {recievedPacket.Message}");
        }

        public static void PrintBytes(byte[] recievedBytes)
        {
            string recievedString = Encoding.UTF8.GetString(recievedBytes);
            Console.WriteLine($"[SYSTEM] Recieved -> {recievedString}");
        }

        public static void PrintMessage(string message)
        {
            Console.WriteLine(message);
        }

        public static void PrintOptions()
        {
            Console.WriteLine("To Chat type: 'CHAT' ...");
            Console.WriteLine("To Create a new Group type: 'NEW G' ...");
            Console.WriteLine("To Join a Group type: 'JOIN G' ...");
            Console.WriteLine("To Leave a Group type: 'LEAVE G' ...");
            Console.WriteLine("To Display Options type: 'OPTIONS' ...");
            Console.WriteLine("NOTE: Type 'CLEAR' to clear the screen at any time\n");
        }

        public static void PrintDictKeys(string intro_message, Dictionary<string, IPEndPoint> dict)
        {
            Console.WriteLine(intro_message);
            foreach (string str in dict.Keys)
            {
                Console.WriteLine($"\t- {str}");
            }
        }
    }
}
