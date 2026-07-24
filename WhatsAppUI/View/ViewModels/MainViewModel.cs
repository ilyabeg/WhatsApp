using Client.Client_Related;
using Client.Events;
using Client.Interfaces;
using Client.Client_Related;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

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
        public IClient ThisClient { get; }

        public MainViewModel(IClient thisClient)
        {
            ChatItems = new();
            ChatViewModel = new ChatViewModel(thisClient, _selectedChat); // <- pass in me (the client)

            ThisClient = thisClient;
            ThisClient.OnUserChanged += UserChangedHandler;
            ThisClient.OnGroupsChanged += GroupsChangedHandler;

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

        /// <summary>
        /// Event handler to add and remove the changed groups provided from the Model to update the UI
        /// </summary>
        private void GroupsChangedHandler(object sender, GroupChangedEventArgs e)
        {
            AddNewGroups(e.GroupChats);
            RemoveGroups(e.GroupChats);
        }

        private void AddNewGroups(List<GroupChat> changedGroups)
        {
            foreach (GroupChat group in changedGroups)
            {
                // if ChatItems doesn't have the group then add it
                if (ChatItemAt(group.ChatItemName) == null)
                {
                    ChatItems.Add(group);
                }
            }
        }

        private void RemoveGroups(List<GroupChat> changedGroups)
        {
            // filter UI connections list to only the grooup
            List<GroupChat> onlyGroups = ChatItems.OfType<GroupChat>().ToList();

            foreach (GroupChat group in onlyGroups)
            {
                // if the UI group chat was removed from the actual groups in the Model,
                // then remove the group from the UI
                if (!IsInGroups(group, changedGroups))
                    ChatItems.Remove(group);
            }
        }

        /// <summary>
        /// Returns true if the provided group is in the list of GroupChats
        /// </summary>
        private bool IsInGroups(GroupChat providedGroup, List<GroupChat> groups)
        {
            foreach (GroupChat group in groups)
            {
                if (group.ChatItemName == providedGroup.ChatItemName) return true;
            }
            return false;
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
            List<string> activeUsers = ThisClient.GetActiveUsers();

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
