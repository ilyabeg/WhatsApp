using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Events
{
    public enum State { Cennecting, Disconnecting }

    internal class UserChangedEventArgs : EventArgs
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
