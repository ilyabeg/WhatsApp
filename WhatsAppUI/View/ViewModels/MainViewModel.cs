using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Client.Interfaces;

namespace WhatsAppUI.View.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        // Users and Groups list
        public ObservableCollection<IChatItem> ChatItems { get; set; }


        // the chat and message input ViewModel
        public ChatViewModel ChatViewModel { get; set; }


        private IChatItem _selectedChat;
        public IChatItem SelectedChat
        {
            get { return _selectedChat; }
            set 
            {
                _selectedChat = value;
                OnPropertyChanged();

                ChatViewModel.Clear(_selectedChat);
            }
        }

        public MainViewModel()
        {
            ChatItems = new();
            ChatViewModel = new ChatViewModel();
        }        

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertName));
        }
    }
}
