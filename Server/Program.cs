using Server.Servers;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpServer server = new TcpServer();
            server.Listen();
        }
    }
}
