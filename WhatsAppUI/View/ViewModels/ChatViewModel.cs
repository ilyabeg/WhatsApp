using Client.Events;
using Client.Interfaces;
using Client.UDP;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WhatsAppUI.View.Helpers;

namespace WhatsAppUI.View.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        // selected user
        private IChatItem _remoteClient;
        private IClient _thisClient;
        public ObservableCollection<MessageBubble> Messages { get; set; }

        // public crash event
        public event Action<string> OnSystemCrash;

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
        public ChatViewModel(IClient thisClient, IChatItem selectedClient)
        {
            Messages = new();
            SendCommand = new RelayCommand(ExecuteSend, CanExecuteSend);
            ClearCommand = new RelayCommand(ExecuteClear, CanExecuteClear);

            _thisClient = thisClient;
            _thisClient.OnMessageReceived += MessageRecievedHandler;
            _thisClient.OnSystemError += SystemErrorHandler;

            _remoteClient = selectedClient;
        }

        // send command
        private void ExecuteSend(object parameter)
        {
            // if client is using UDP allow him to send Broadcast
            if (this.Message.Trim().StartsWith("@all") && _thisClient is ClientUDP udpClient)
                udpClient.SendBroadcast(this.Message);

            // send the message to the reomte client
            else
                _thisClient.SendUnicastMessage(_remoteClient.ChatItemName, this.Message);

            Messages.Add(new MessageBubble($"Me: {this.Message}", true)); // <- add the message that was sent by me
            this.Message = "";
        }
        private bool CanExecuteSend(object parameter) => !string.IsNullOrWhiteSpace(this.Message) && _remoteClient != null;


        // Message Recieved event handler
        private void MessageRecievedHandler(object sender, MessageRecievedEventArgs e)
        {
            if (e.Sender == _thisClient.ChatItemName) return; // not get my own messages

            // display message only if the message is from my current peer or a Broadcast was sent
            if (e.Message.StartsWith("@all") || e.Sender == _remoteClient.ChatItemName)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(new MessageBubble($"{e.Sender}: {e.Message}", false)); // <- false = Message sent NOT by me
                });
            }            
        }

        // System Error handler
        private void SystemErrorHandler(object sender, SystemErrorEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnSystemCrash?.Invoke(e.ErrorMessage);
            });
        }

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
