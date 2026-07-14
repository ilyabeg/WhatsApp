using Server.Helpers;
using Server.IO_Handlers;
using Server.Objects;
using System.Net;
using System.Text;

namespace Server.Servers
{
    internal class UdpInputHandler
    {
        UdpOutputHandler _outputHandler;
        public UdpInputHandler(UdpOutputHandler handler)
        {
            _outputHandler = handler;
        }

        public void HandleMessage(string msg, IPEndPoint clientEndPoint, string sender)
        {
            string recieverID = "#", actualMsg = "#";
            try
            {
                if (msg == null || msg.IsWhiteSpace())
                    throw new Exception();

                recieverID = MessageProcessor.GetRecieverID(msg);
                actualMsg = MessageProcessor.GetActualMsg(msg);

                IPEndPoint reciever = UdpServer._all_clients[recieverID];

                byte[] message = Encoding.UTF8.GetBytes($"[{sender}]: {actualMsg}");
                UdpServer._listener.Send(message, message.Length, reciever);
            }
            catch (Exception)
            {
                _outputHandler.SendInvalidMessage(clientEndPoint);
            }
        }

        public void JoinGroupChat(string msg, IPEndPoint clientEndPoint)
        {
            try
            {
                string groupName = MessageProcessor.GetGroupName(msg);

                foreach (GroupChat group in UdpServer._group_chats)
                {
                    if (group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                    {
                        // run group in the background and change client's port to the group's port
                        Task.Run(group.RunGroup);
                        clientEndPoint.Port = group.PortNumber;
                        return;
                    }
                }
            }
            catch
            { }
            _outputHandler.SendInvalidMessage(clientEndPoint); // invalid if no group was found or exception cought
        }                
    }
}
