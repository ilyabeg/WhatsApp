using System.Windows.Controls;
using WhatsAppUI.View.ViewModels;

namespace WhatsAppUI.View.UserControls
{
    public partial class LoginForm : UserControl
    {
        public LoginForm()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }        
    }
}
