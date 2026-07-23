using Client.Interfaces;

namespace Client.Client_Related
{
    public class ChatUser : IChatItem
    {
        public string ChatItemName { get; set; }

        public ChatUser(string name)
        {
            ChatItemName = name;
        }
    }
}
