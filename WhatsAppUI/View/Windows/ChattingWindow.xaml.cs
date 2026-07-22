using System.Windows;
using WhatsAppUI.View.ViewModels;

namespace WhatsAppUI.View.Windows
{
    public partial class ChattingWindow : Window
    {
        public ChattingWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(); // <- connect to view model
        }
    }
}
