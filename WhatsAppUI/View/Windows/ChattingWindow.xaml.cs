using Client.Interfaces;
using System.Windows;
using WhatsAppUI.View.ViewModels;

namespace WhatsAppUI.View.Windows
{
    public partial class ChattingWindow : Window
    {
        public ChattingWindow(IClient thisClient)
        {
            InitializeComponent();

            MainViewModel mainViewModel = new MainViewModel(thisClient);
            mainViewModel.ChatViewModel.OnSystemCrash += ShowSystemError; // ChatViewModel System Errors

            // attach window closing event to DisconnectClient method inside each client to remove from View
            this.Closing += (s, e) =>
            {
                thisClient.DisconnectClient();
            };

            DataContext = mainViewModel;
        }

        private void ShowSystemError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "SYSTEM ERROR", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
        }
    }
}
