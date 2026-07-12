using Client.Clients;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ClientTCP client = new ClientTCP();
            client.Start();
        }
    }
}
