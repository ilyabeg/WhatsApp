using System.Windows;

namespace WhatsAppUI.View.Windows
{
    public enum Option { NewGroup, JoinGroup, LeaveGroup }

    public partial class GroupOptionsWindow : Window
    {
        public event Action<string> OnGroupSelected; // event to send group selection

        public GroupOptionsWindow(Option groupOption, List<string> availableGroups = null)
        {
            InitializeComponent();

            // creating group
            if (groupOption == Option.NewGroup)
            {
                txtBlock.Text = "Enter GroupChat name:";
                inputTxt.Visibility = Visibility.Visible;
                groupsListBox.Visibility = Visibility.Collapsed; // hide listbox
                noGroupsTxtBlock.Visibility = Visibility.Collapsed; // hide none message
            }

            // join group
            else if (groupOption == Option.JoinGroup)
            {
                txtBlock.Text = "Select group:";
                inputTxt.Visibility = Visibility.Hidden; // hide list box
                groupsListBox.Visibility = Visibility.Visible;
                noGroupsTxtBlock.Visibility = Visibility.Collapsed; // hide none message

                if (availableGroups.Count == 0)
                    noGroupsTxtBlock.Visibility = Visibility.Visible; // hide none message
                else
                    groupsListBox.ItemsSource = availableGroups;
            }

            // leave group
            else if (groupOption == Option.LeaveGroup)
            {
                txtBlock.Text = "Select group:";
                inputTxt.Visibility = Visibility.Hidden; // hide list box
                groupsListBox.Visibility = Visibility.Visible;
                noGroupsTxtBlock.Visibility = Visibility.Collapsed; // hide none message

                if (availableGroups.Count == 0)
                    noGroupsTxtBlock.Visibility = Visibility.Visible; // hide none message
                else
                    groupsListBox.ItemsSource = availableGroups;
            }
        }

        public void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string? selectedGroup = null;

            if (inputTxt.Visibility == Visibility.Visible)
            {
                selectedGroup = inputTxt.Text.Trim();
            }
            else
            {
                selectedGroup = groupsListBox.SelectedItem as string;
            }

            // if user didn't type/select anything don't let him click OK
            if (string.IsNullOrWhiteSpace(selectedGroup))
            {
                MessageBox.Show("Please provide a valid group.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return;
            }

            OnGroupSelected?.Invoke(selectedGroup); // send selection via invoking the event
            Close(); // close window
        }

        public void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(); // close window
        }
    }
}
