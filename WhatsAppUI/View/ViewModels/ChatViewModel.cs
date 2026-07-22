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
        public ObservableCollection<string> Messages { get; set; }

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
        public ChatViewModel()
        {
            Messages = new();
            SendCommand = new RelayCommand(ExecuteSend, CanExecuteSend);
        }

        private void ExecuteSend(object parameter)
        {
            //Send(this.Message, _remoteClient)

            Messages.Add($"Me: {this.Message}");
            this.Message = "";
        }
        private bool CanExecuteSend(object parameter) => !string.IsNullOrWhiteSpace(this.Message) && _remoteClient != null;

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
