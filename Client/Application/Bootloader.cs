using Client.Interfaces;
using Client.TCP;
using Client.UDP;

namespace Client.Application
{
    internal class Bootloader
    {
        private static IClient _client;

        public static void Boot()
        {
            char desition = GetDesition();
            if (desition == 'T' || desition == 't')
                BootTCP();
            else
                BootUDP();
        }

        private static char GetDesition()
        {
            char desition;
            do
            {
                Console.WriteLine("[SYSTEM] Enter protocol ('T': TCP or 'U': UDP) >");
                desition = Console.ReadKey().KeyChar;
                Console.WriteLine();
            }
            while (desition != 'T' && desition != 't' && desition != 'U' && desition != 'u');
            return desition;
        }

        private static void BootUDP()
        {
            _client = new ClientUDP();
            _client.Start();
        }

        private static void BootTCP()
        {
            _client = new ClientTCP();
            _client.Start();
        }
    }
}
