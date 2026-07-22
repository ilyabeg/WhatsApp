using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Events
{
    // for who is the message intended
    public enum MessageKind { UserMessage, GroupMessage }

    public class MessageRecievedEventArgs : EventArgs
    {
        public string Sender { get; }
        public string Message { get; }
        public MessageKind Kind { get; }

        public MessageRecievedEventArgs(string sender, string message, MessageKind messageKind)
        {
            Sender = sender;
            Message = message;
            Kind = messageKind;
        }
    }
}
