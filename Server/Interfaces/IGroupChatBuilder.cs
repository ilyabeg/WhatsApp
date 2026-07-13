
namespace Server.Interfaces
{
    internal interface IGroupChatBuilder
    {
        public IGroupChatBuilder NewGroup();
        public IGroupChatBuilder SetName(string name);
        public IGroupChatBuilder SetPort(int portNum);
        public IGroupChatBuilder SetPrivacy(bool isPrivate);
    }
}
