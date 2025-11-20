using System.Windows;

namespace SwiftOpsToolbox.Views
{
    public partial class MarkdownWindow : Window
    {
        public MarkdownWindow()
        {
            InitializeComponent();
        }

        public void Navigate(string path)
        {
            PreviewBrowser.Navigate(path);
        }
    }
}