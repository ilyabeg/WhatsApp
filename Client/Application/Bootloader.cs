using Client.Interfaces;
using Client.TCP;
using Client.UDP;

namespace Client.Application
{
    internal class Bootloader
    {
        private static IClient _client;
        public static void Boot(char desition) 
        {
            if (desition == 'T' || desition == 't')
                BootTCP();
            else
                BootUDP();
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
