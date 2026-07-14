
namespace Server.Interfaces
{
    internal interface IGroupChatBuilder
    {
        public IGroupChatBuilder NewGroup();        
        public IGroupChatBuilder SetConfig();
        public IGroupChatBuilder SetPort(int portNum);
        public IGroupChatBuilder SetName(string name);        
        public IGroupChatBuilder SetPrivacy(bool isPrivate);
    }
}
