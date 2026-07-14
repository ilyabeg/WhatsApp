
using System.Net;

namespace Server.Interfaces
{
    internal interface IGroupChatBuilder
    {
        public IGroupChatBuilder NewGroup();        
        public IGroupChatBuilder SetConfig();
        public IGroupChatBuilder SetEndPoint(int portNum, IPAddress ip);
        public IGroupChatBuilder SetName(string name);        
        public IGroupChatBuilder SetPrivacy(bool isPrivate);
    }
}
