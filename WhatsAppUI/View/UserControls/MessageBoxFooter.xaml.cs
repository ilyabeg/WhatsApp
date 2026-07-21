using System.Windows;
using System.Windows.Controls;

namespace WhatsAppUI.View.UserControls
{
    public partial class MessageBoxFooter : UserControl
    {
        public MessageBoxFooter()
        {
            InitializeComponent();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            InputTxt.Clear();
            InputTxt.Focus(); // focus text bar after clear
        }

        private void InputTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            // show place holder text when text box is empty and hide when not empty
            if (string.IsNullOrEmpty(InputTxt.Text))
                PlaceHolderTxt.Visibility = Visibility.Visible;
            else
                PlaceHolderTxt.Visibility = Visibility.Hidden;
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string message = InputTxt.Text;

            if (!string.IsNullOrEmpty(message))
            {
                // send to remote user...
            }
        }
    }
}
