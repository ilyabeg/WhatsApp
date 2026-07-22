namespace Client.Interfaces
{
    internal interface IClient
    {
        public void SendUnicastMessage(string remoteClientName, string message);
        public void Connect(string username);
    }
}
