using Client.Interfaces;

namespace Client.UDP
{
    public class GroupChat : IChatItem
    {
        public string ChatItemName { get; set; }
        public int MembersCount { get; set; }

        public GroupChat(string name)
        {
            ChatItemName = name;
            MembersCount = 0;
        }
    }
}
