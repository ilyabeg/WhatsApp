using Server.Interfaces;
using Server.Objects;
using System.Net;
using System.Net.Sockets;

namespace Server.Builders
{
    internal class GroupChatBuilder : IGroupChatBuilder
    {
        private GroupChat _group;

        public IGroupChatBuilder NewGroup()
        {
            _group = new GroupChat();
            _group.GroupListener = new UdpClient();
            return this;
        }        

        public IGroupChatBuilder SetConfig()
        {
            _group.GroupListener.EnableBroadcast = true; // <- enable broadcasting to group clients
            _group.GroupListener.ExclusiveAddressUse = false; // <- non exclusive addresses
            _group.GroupListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);            
            return this;
        }

        public IGroupChatBuilder SetEndPoint(int portNum, IPAddress ip)
        {
            _group.EndPoint = new IPEndPoint(ip, portNum);

            _group.GroupListener.Client.Bind(_group.EndPoint); // <- bind the group listener to the end point
            return this;
        }

        public IGroupChatBuilder SetName(string name)
        {
            _group.Name = name;
            return this;
        }        

        public IGroupChatBuilder SetPrivacy(bool isPrivate)
        {
            _group.IsPrivate = isPrivate;
            return this;

        }

        public GroupChat Build() { return _group; }
    }
}
