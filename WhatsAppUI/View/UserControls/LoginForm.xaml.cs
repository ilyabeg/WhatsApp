using System.Windows;
using System.Windows.Controls;
using WhatsAppUI.View.Windows;

namespace WhatsAppUI.View.UserControls
{
    public partial class LoginForm : UserControl
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInfo.Text;
            UsernameInfo.Clear();

            if (false) // <- Username Authorization...
            {
                LoginTxt.Text = "Username already taken. Please re-enter:";
            }
            else
            {
                // display UDP/TCP choice
                MessageBox.Show("Would you like to use UDP Communication? (No = TCP Communication)", "Protocol Choice", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.TryAgain);

                // open chatting window
                ChattingWindow chattingWindow = new();
                chattingWindow.Show();
                
                // close main window
                Window parentWindow = Window.GetWindow(this);
                parentWindow?.Close();
            }
        }
    }
}
