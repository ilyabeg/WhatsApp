using Client.Interfaces;
using System.Windows;
using WhatsAppUI.View.ViewModels;
using WhatsAppUI.View.Windows;

namespace WhatsAppUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // get user protocol choice
            bool choice = ChooseProtocol();

            LoginViewModel viewModel = new LoginViewModel(choice);

            viewModel.OnLoginSuccess += OpenChatWindow; // <- subscribe to login success event

            DataContext = viewModel;
        }

        /// <summary>
        /// View Models cannot access windows and UI components directly because the shouldn't know about their existance,
        /// hence we manipulate the messageBox and window openning and closing here because if the view model were to do that, then that  
        /// would have contradicted the MVVM design pattern ...
        /// </summary>
        private void OpenChatWindow(IClient newClient)
        {                                    
            // open chat window
            ChattingWindow chattingWindow = new(newClient);
            chattingWindow.Show();

            Close(); // close this window
        }

        /// <summary>
        /// Display Protocol choice to client and return true if chosen UDP, else returns false
        /// </summary>
        private bool ChooseProtocol()
        {
            // TCP/UDP choice
            MessageBoxResult result = MessageBox.Show("Would you like to use UDP Protocol? (NO = TCP Protocol)", "Protocol Choice", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.TryAgain);
            return result == MessageBoxResult.Yes ? true : false;
        }
    }
}