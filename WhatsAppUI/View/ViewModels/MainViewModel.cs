using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Client.Events;
using Client.Interfaces;
using Client.Client_Related;

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

        // current client (ME)
        private IClient _thisClient;

        public MainViewModel(IClient thisClient)
        {
            ChatItems = new();
            ChatViewModel = new ChatViewModel(thisClient, _selectedChat); // <- pass in me (the client)

            _thisClient = thisClient;
            _thisClient.OnUserChanged += UserChangedHandler;

            AddActiveUsers();
        }        

        // event handlers
        private void UserChangedHandler(object sender, UserChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // new user connected
                if (e.State == State.Connecting)
                {
                    if (ChatItemAt(e.UserName) == null) // <- if not in the ChatItems list
                    {
                        ChatItems.Add(new ChatUser(e.UserName));
                    }                    
                }

                // user disconnected
                else if (e.State == State.Disconnecting)
                {
                    IChatItem? user = ChatItemAt(e.UserName);
                    if (user != null)
                    {
                        ChatItems.Remove(user);
                    }
                }
            });
        }

        private IChatItem? ChatItemAt(string name)
        {
            foreach (IChatItem item in ChatItems)
            {
                if (item.ChatItemName == name) return item;
            }
            return null;
        }

        /// <summary>
        /// Adds all current active users to the ChatItems list at the begining of each connection
        /// </summary>
        private void AddActiveUsers()
        {
            List<string> activeUsers = _thisClient.GetActiveUsers();

            foreach (string username in activeUsers)
            {
                ChatItems.Add(new ChatUser(username));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertName));
        }
    }
}
