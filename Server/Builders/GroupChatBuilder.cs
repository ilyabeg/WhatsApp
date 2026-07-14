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
            _group.GroupListener.ExclusiveAddressUse = false; // <- non exclusive addresses
            _group.GroupListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);            
            return this;
        }

        public IGroupChatBuilder SetPort(int portNum)
        {
            _group.PortNumber = portNum;
            _group.GroupListener.Client.Bind(new IPEndPoint(IPAddress.Any, _group.PortNumber));
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
