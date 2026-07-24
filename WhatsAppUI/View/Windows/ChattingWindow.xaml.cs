using Client.Interfaces;
using Client.UDP;
using System.Windows;
using WhatsAppUI.View.ViewModels;

namespace WhatsAppUI.View.Windows
{
    public partial class ChattingWindow : Window
    {
        private IClient _thisClient;

        public ChattingWindow(IClient thisClient)
        {
            InitializeComponent();

            MainViewModel mainViewModel = new MainViewModel(thisClient);
            mainViewModel.ChatViewModel.OnSystemCrash += ShowSystemError; // ChatViewModel System Errors

            _thisClient = thisClient; // save current client
            mainViewModel.OnNewGroupChat += NewGroupClicked;
            mainViewModel.OnJoinGroupChat += JoinGroupClicked;
            mainViewModel.OnLeaveGroupChat += LeaveGroupClicked;

            // attach window closing event to DisconnectClient method inside each client to remove from View
            this.Closing += (s, e) =>
            {
                thisClient.DisconnectClient();
            };

            DataContext = mainViewModel;
        }

        // groupchat options button event handlers
        private void NewGroupClicked()
        {
            GroupOptionsWindow groupOptionWindow = new GroupOptionsWindow(Option.NewGroup);

            // selected group event handler
            groupOptionWindow.OnGroupSelected += (groupName) =>
            {
                if (_thisClient is ClientUDP thisUdpClient)
                {
                    thisUdpClient.CreateGroup(groupName);
                }
            };

            groupOptionWindow.Show(); // open window
        }

        private void JoinGroupClicked(List<string> availableGroups)
        {
            GroupOptionsWindow groupOptionWindow = new GroupOptionsWindow(Option.JoinGroup, availableGroups);

            // selected group event handler
            groupOptionWindow.OnGroupSelected += (groupName) =>
            {
                if (_thisClient is ClientUDP thisUdpClient)
                {
                    thisUdpClient.JoinGroup(groupName);
                }
            };

            groupOptionWindow.Show(); // open window
        }

        private void LeaveGroupClicked(List<string> availableGroups)
        {
            GroupOptionsWindow groupOptionWindow = new GroupOptionsWindow(Option.LeaveGroup, availableGroups);

            // selected group event handler
            groupOptionWindow.OnGroupSelected += (groupName) =>
            {
                if (_thisClient is ClientUDP thisUdpClient)
                {
                    thisUdpClient.LeaveGroup(groupName);
                }
            };

            groupOptionWindow.Show(); // open window
        }

        private void ShowSystemError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "SYSTEM ERROR", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
        }
    }
}
