using System;

namespace Client.Events
{
    public enum State { Connecting, Disconnecting }

    public class UserChangedEventArgs : EventArgs
    {
        public string UserName {  get; }
        public State State { get; }

        public UserChangedEventArgs(string userName, State state)
        {
            UserName = userName;
            State = state;
        }
    }
}
