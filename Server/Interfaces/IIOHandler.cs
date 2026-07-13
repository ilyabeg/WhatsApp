
using System.Net;

namespace Server.Interfaces
{
    internal interface IIOHandler<T>
    {
        public void HandleMessage(string msg, T client, string sender);
        public void DisplayConnectedClients(T client);
        public void PrintMessageDetails(string msg, string clientID);
    }
}
