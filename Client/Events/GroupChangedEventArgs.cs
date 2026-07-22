using Client.UDP;

namespace Client.Events
{
    internal class GroupChangedEventArgs : EventArgs
    {
        public List<GroupChat> GroupChats { get; }

        public GroupChangedEventArgs(List<GroupChat> groupChats)
        {
            GroupChats = groupChats;
        }
    }
}
