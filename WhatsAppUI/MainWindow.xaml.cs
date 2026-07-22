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

            LoginViewModel viewModel = new LoginViewModel();
            viewModel.OnLoginSuccess += OpenChatWindow; // <- subscribe to login success event

            DataContext = viewModel;
        }

        /// <summary>
        /// View Models cannot access windows and UI components directly because the shouldn't know about their existance,
        /// hence we manipulate the messageBox and window openning and closing here because if the view model were to do that, then that  
        /// would have contradicted the MVVM design pattern ...
        /// </summary>
        private void OpenChatWindow()
        {
            // display UDP/TCP choice
            MessageBoxResult res = MessageBox.Show("Would you like to use UDP Communication? (No = TCP Communication)", "Protocol Choice", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.TryAgain);

            bool isUdp = (res == MessageBoxResult.Yes);            
            
            //IChatNetworkModel selectedModel = isUdp ? new UdpChatModel() : new TcpChatModel();
            // ChatViewModel chatVm = new ChatViewModel(selectedModel); // <- inject chosen chat model

            // open chat window
            ChattingWindow chattingWindow = new();
            chattingWindow.Show();

            Close(); // close this window
        }
    }
}