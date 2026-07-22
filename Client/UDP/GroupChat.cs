using Client.Interfaces;

namespace Client.UDP
{
    internal class GroupChat : IChatItem
    {
        public string ChatItemName { get; set; }
        public int MembersCount { get; set; }
    }
}
