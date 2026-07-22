using Client.UDP;

namespace Client.Events
{
    public class GroupChangedEventArgs : EventArgs
    {
        public List<GroupChat> GroupChats { get; }

        public GroupChangedEventArgs(List<GroupChat> groupChats)
        {
            GroupChats = groupChats;
        }
    }
}
