using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Events
{
    internal class SystemErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; }

        public SystemErrorEventArgs(string message)
        {
            ErrorMessage = message;
        }
    }
}
