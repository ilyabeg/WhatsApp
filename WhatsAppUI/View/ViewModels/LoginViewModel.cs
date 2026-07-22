using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WhatsAppUI.View.Helpers;
using WhatsAppUI.View.Windows;

namespace WhatsAppUI.View.ViewModels
{
    internal class LoginViewModel : INotifyPropertyChanged
    {
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
        public LoginViewModel()
        {
            LoginText = "Please enter your Username:";

            // init Register Button Binded command
            RegisterCommand = new RelayCommand(Register, CanRegister);
        }

        private void Register(object parameter)
        {
            bool isTaken = false;//MyModel.Login(this.Username);

            if (!isTaken) // <- Username Authorization from Model...
            {
                LoginText = "Username already taken. Please re-enter:";
                Username = "";
            }
            else
            {
                //// display UDP/TCP choice
                //MessageBox.Show("Would you like to use UDP Communication? (No = TCP Communication)", "Protocol Choice", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.TryAgain);

                //// open chatting window
                //ChattingWindow chattingWindow = new();
                //chattingWindow.Show();

                ////close main window
                //Window parentWindow = Window.GetWindow(this);
                //parentWindow?.Close();
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
