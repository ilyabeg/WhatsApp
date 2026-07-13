using Server.Interfaces;
using Server.Objects;
using System.Net.Sockets;

namespace Server.Builders
{
    internal class GroupChatBuilder : IGroupChatBuilder
    {
        private GroupChat _group;

        public IGroupChatBuilder NewGroup()
        {
            _group = new GroupChat();
            return this;
        }

        public IGroupChatBuilder SetName(string name)
        {
            _group.Name = name;
            return this;
        }

        public IGroupChatBuilder SetPort(int portNum)
        {
            _group.GroupListener = new UdpClient(portNum);
            _group.PortNumber = portNum;
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
