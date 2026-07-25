using Client.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client.Client_Related
{
    public class GroupChat : IChatItem, INotifyPropertyChanged
    {
        public string ChatItemName { get; set; }
        private int _membersCount;
        public int MembersCount 
        {
            get { return _membersCount; }
            set
            {
               _membersCount = value;
               OnPropertyChanged();
            } 
        }

        // empty constructor for json deserialization
        public GroupChat() { }
        public GroupChat(string name)
        {
            ChatItemName = name;
            MembersCount = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
