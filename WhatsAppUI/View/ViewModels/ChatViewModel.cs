using Client.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WhatsAppUI.View.Helpers;

namespace WhatsAppUI.View.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        // selected user
        private IChatItem _remoteClient;
        public ObservableCollection<MessageBubble> Messages { get; set; }

        // user input
        private string _message;
        public string Message
        {
            get { return _message; }
            set 
            { 
                _message = value;
                OnPropertyChanged();
            }
        }

        public ICommand SendCommand { get; } // <- send button click command
        public ICommand ClearCommand { get; } // <- clear msgBox command
        public ChatViewModel()
        {
            Messages = new();
            SendCommand = new RelayCommand(ExecuteSend, CanExecuteSend);
            ClearCommand = new RelayCommand(ExecuteClear, CanExecuteClear);
        }

        // send command
        private void ExecuteSend(object parameter)
        {
            //Send(this.Message, _remoteClient)

            Messages.Add(new MessageBubble(this.Message, true)); // <- add the message that was sent by me
            this.Message = "";
        }
        private bool CanExecuteSend(object parameter) => !string.IsNullOrWhiteSpace(this.Message); // && _remoteClient != null;


        // clear command
        private void ExecuteClear(object parameter) => this.Message = "";
        private bool CanExecuteClear(object parameter) => !string.IsNullOrWhiteSpace(this.Message);


        public void Clear(IChatItem remoteClient)
        {
            _remoteClient = remoteClient;
            Messages.Clear(); // <- clear connection
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertName));
        }
    }
}
