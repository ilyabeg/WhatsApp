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
        /// hence we manipulate the window openning and closing here because if the view model were to do that, then that  
        /// would have contradicted the MVVM design pattern ...
        /// </summary>
        private void OpenChatWindow()
        {
            ChattingWindow chattingWindow = new();
            chattingWindow.Show();

            Close(); // close this window
        }
    }
}