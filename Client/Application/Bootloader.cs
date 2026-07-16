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
            if (GetDesition() == 'T' || GetDesition() == 't')
                BootTCP();
            else
                BootUDP();
        }

        private static char GetDesition()
        {
            char desition = '0';
            while ((desition != 'T' && desition != 't') || (desition != 'U' && desition != 'u'))
            {
                Console.WriteLine("[SYSTEM] Enter protocol ('T': TCP or 'U': UDP) >");
                desition = Console.ReadKey().KeyChar;
                Console.WriteLine();
            }
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
