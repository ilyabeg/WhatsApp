using System.Windows;
using WhatsAppUI.View.ViewModels;

namespace WhatsAppUI.View.Windows
{
    public partial class ChattingWindow : Window
    {
        public ChattingWindow()
        {
            InitializeComponent();
            this.DataContext = new ChatViewModel(); // <- connect to view model
        }
    }
}
