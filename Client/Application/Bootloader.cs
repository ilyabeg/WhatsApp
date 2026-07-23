using Client.Interfaces;
using Client.TCP;
using Client.UDP;

namespace Client.Application
{
    public class Bootloader
    {
        public static IClient BootClient(bool isUDP)
        {
            return isUDP ? new ClientUDP() : new ClientTCP();
        }
    }
}
