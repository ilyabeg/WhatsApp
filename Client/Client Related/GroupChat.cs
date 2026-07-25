using Client.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

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

        // boolean to hide/show groups in the UI ONLY if a user is a member
        [JsonIgnore]
        public bool IsMember { get; set; } = false;

        // empty constructor for json deserialization
        public GroupChat() { }
        public GroupChat(string name)
        {
            ChatItemName = name;
            MembersCount = 0;
        }
        public GroupChat(string name, bool isMember)
        {
            ChatItemName = name;
            IsMember = isMember;
            MembersCount = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
