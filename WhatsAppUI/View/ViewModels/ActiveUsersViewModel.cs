using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WhatsAppUI.View.ViewModels
{
    public class ActiveUsersViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        //public ObservableCollection<IChatItem> ActiveChats { get; set; }

        //private IChatItem _selectedChat;
        //public IChatItem SelectedChat
        //{
        //    get { return _selectedChat; }
        //    set
        //    {
        //        if (_selectedChat != value)
        //        {
        //            _selectedChat = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        //public ActiveUsersViewModel()
        //{
        //    ActiveChats = new();
        //}

        private void OnPropertyChanged()
        {

        }
    }
}
