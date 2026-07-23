using Client.Events;

namespace Client.Interfaces
{
    public interface IClient : IChatItem
    {
        public void SendUnicastMessage(string remoteClientName, string message);
        public bool Connect(string username);
        public void DisconnectClient();

        public List<string> GetActiveUsers();

        // define public events for ViewModel to subscribe to
        public event EventHandler<MessageRecievedEventArgs> OnMessageReceived;
        public event EventHandler<UserChangedEventArgs> OnUserChanged;
        public event EventHandler<GroupChangedEventArgs> OnGroupsChanged;
        public event EventHandler<SystemErrorEventArgs> OnSystemError;
    }
}
