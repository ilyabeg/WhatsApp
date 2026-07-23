using Client.Application;
using Client.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WhatsAppUI.View.Helpers;

namespace WhatsAppUI.View.ViewModels
{
    internal class LoginViewModel : INotifyPropertyChanged
    {
        public event Action<IClient> OnLoginSuccess; // <- event for switching from main window into the chat window
        private bool _isUDP; // boolean to know which client to boot

        private string _username = "";
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        private string _loginTxt;
        public string LoginText
        {
            get { return _loginTxt; }
            set 
            { 
                _loginTxt = value;
                OnPropertyChanged();
            }
        }

        public ICommand RegisterCommand { get; }
        public LoginViewModel(bool isUdpProtocol)
        {
            _isUDP = isUdpProtocol;

            LoginText = "Please enter your Username:";

            // init Register Button Binded command
            RegisterCommand = new RelayCommand(Register, CanRegister);
        }

        private void Register(object parameter)
        {
            IClient newClient = Bootloader.BootClient(_isUDP);

            // Username authorization using Model logic
            bool isFree = newClient.Connect(this.Username);

            if (!isFree) // <- Username Authorization from Model...
            {
                LoginText = "Username already taken. Please re-enter:";
                Username = "";
            }
            else
            {
                // open chatting window and close main window
                OnLoginSuccess.Invoke(newClient);                
            }
        }
        private bool CanRegister(object parameter) => !string.IsNullOrEmpty(this.Username);


        // fire property changed event
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertName));
        }
    }
}
