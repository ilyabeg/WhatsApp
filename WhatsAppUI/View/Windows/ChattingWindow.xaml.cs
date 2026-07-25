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
            this.Title = $"WhatsApp (username: {thisClient.ChatItemName})";

            MainViewModel mainViewModel = new MainViewModel(thisClient);
            mainViewModel.ChatViewModel.OnSystemCrash += ShowSystemError; // ChatViewModel System Errors

            mainViewModel.OnNewGroupChat += NewGroupClicked;
            mainViewModel.OnJoinGroupChat += JoinGroupClicked;
            mainViewModel.OnLeaveGroupChat += LeaveGroupClicked;

            // attach window closing event to Disconnect method inside the MainViewModel to remove clients from View
            this.Closing += (s, e) =>
            {
                mainViewModel.Disconnect();
            };

            DataContext = mainViewModel;
        }

        // groupchat options button event handlers to execute the actual group option INSIDE the ViewModel NOT the view
        private void NewGroupClicked()
        {
            GroupOptionsWindow groupOptionWindow = new GroupOptionsWindow(Option.NewGroup);

            // selected group event handler
            groupOptionWindow.OnGroupSelected += (groupName) =>
            {
                if (DataContext is MainViewModel mainViewModel)
                    mainViewModel.ExecuteNewGroup(groupName);
            };

            groupOptionWindow.Show(); // open window
        }
        private void JoinGroupClicked(List<string> availableGroups)
        {
            GroupOptionsWindow groupOptionWindow = new GroupOptionsWindow(Option.JoinGroup, availableGroups);

            // selected group event handler
            groupOptionWindow.OnGroupSelected += (groupName) =>
            {
                if (DataContext is MainViewModel mainViewModel)
                    mainViewModel.ExecuteJoinGroup(groupName);
            };

            groupOptionWindow.Show(); // open window
        }
        private void LeaveGroupClicked(List<string> availableGroups)
        {
            GroupOptionsWindow groupOptionWindow = new GroupOptionsWindow(Option.LeaveGroup, availableGroups);

            // selected group event handler
            groupOptionWindow.OnGroupSelected += (groupName) =>
            {
                if (DataContext is MainViewModel mainViewModel)
                    mainViewModel.ExecuteLeaveGroup(groupName);
            };

            groupOptionWindow.Show(); // open window
        }

        private void ShowSystemError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "SYSTEM ERROR", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
        }
    }
}
