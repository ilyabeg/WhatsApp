using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Events
{
    public class MessageRecievedEventArgs : EventArgs
    {
        public string Sender { get; }
        public string Message { get; set; }

        public MessageRecievedEventArgs(string sender, string message)
        {
            Sender = sender;
            Message = message;
        }
    }
}
