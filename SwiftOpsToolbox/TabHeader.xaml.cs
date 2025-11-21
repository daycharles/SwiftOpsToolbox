using System.Windows.Controls;

namespace SwiftOpsToolbox
{
    public partial class TabHeader : System.Windows.Controls.UserControl
    {
        public string Title { get; set; }
        public string Icon { get; set; }

        public TabHeader()
        {
            InitializeComponent();
            Loaded += TabHeader_Loaded;
        }

        private void TabHeader_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            IconBlock.Text = Icon;
            TitleBlock.Text = Title;
        }
    }
}