using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace WhatsAppUI.View.UserControls
{
    public partial class ActiveUsersList : UserControl
    {
        //private ObservableCollection<IChatItem> _chatItems;
        //public ObservableCollection<IChatItem> ChatItems
        //{
        //    get { return _chatItems; }
        //    set { _chatItems = value; }
        //}

        public ActiveUsersList()
        {
            //_chatItems = new ObservableCollection<IChatItem>();
            //DataContext = this;
            InitializeComponent();
        }
    }
}
