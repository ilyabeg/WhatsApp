using Server.Helpers;
using Server.IO_Handlers;
using Server.Objects;
using System.Net;

namespace Server.Servers
{
    internal class UdpInputHandler
    {
        //private readonly IPAddress _broadcast_channel = IPAddress.Parse("239.1.1.1");
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

                // broadcast message
                //if (recieverID.Equals("all", StringComparison.OrdinalIgnoreCase))
                //{
                //    IPEndPoint reciever = new IPEndPoint(_broadcast_channel, );

                //}

                // private message
                if (UdpServer._all_clients.ContainsKey(recieverID))
                {
                    // get client's end point
                    IPEndPoint reciever = UdpServer._all_clients[recieverID];
                    _outputHandler.SendMessage(reciever, $"({sender}): {actualMsg}"); // send message to the client
                    return;
                }

                // group message
                else if (UdpServer._group_chats.ContainsKey(recieverID))
                {
                    // get group end point
                    IPEndPoint reciever = new IPEndPoint(IPAddress.Loopback, UdpServer._group_chats[recieverID].EndPoint.Port);
                    _outputHandler.SendMessage(reciever, $"({sender}): {actualMsg}"); // send message to the group
                    return;
                }
            }
            catch (Exception)
            { }
            _outputHandler.SendInvalidMessage(clientEndPoint); // invalid if exception cought or if reciever doesn't exist
        }

        public void JoinGroupChat(string msg, IPEndPoint clientEndPoint)
        {
            try
            {
                string groupName = MessageProcessor.GetGroupName(msg);

                foreach (GroupChat group in UdpServer._group_chats.Values)
                {
                    if (group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                    {
                        group.AddMember(clientEndPoint);
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
