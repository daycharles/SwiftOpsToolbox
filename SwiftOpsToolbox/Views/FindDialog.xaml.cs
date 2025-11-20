using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SwiftOpsToolbox.Views
{
    /// <summary>
    /// Find dialog for searching text in a RichTextBox.
    /// Provides case-sensitive and whole-word search options.
    /// </summary>
    public partial class FindDialog : Window
    {
        private readonly RichTextBox _richTextBox;
        private TextPointer? _currentPosition;

        public FindDialog(RichTextBox richTextBox)
        {
            InitializeComponent();
            _richTextBox = richTextBox;
            _currentPosition = _richTextBox.Document.ContentStart;

            TxtFind.Focus();
            TxtFind.SelectAll();
        }

        private void BtnFindNext_Click(object sender, RoutedEventArgs e)
        {
            string searchText = TxtFind.Text;
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Please enter text to find.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool found = FindNext(searchText, ChkMatchCase.IsChecked == true, ChkWholeWord.IsChecked == true);
            
            if (!found)
            {
                MessageBox.Show($"Cannot find \"{searchText}\"", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
                // Reset to start for next search
                _currentPosition = _richTextBox.Document.ContentStart;
            }
        }

        private bool FindNext(string searchText, bool matchCase, bool wholeWord)
        {
            if (_currentPosition == null)
            {
                _currentPosition = _richTextBox.Document.ContentStart;
            }

            TextPointer? position = _currentPosition;
            
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);
                    
                    StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                    int index = textRun.IndexOf(searchText, comparison);

                    if (index >= 0)
                    {
                        // Check whole word if required
                        if (wholeWord)
                        {
                            bool isWholeWord = true;
                            
                            // Check character before
                            if (index > 0 && char.IsLetterOrDigit(textRun[index - 1]))
                            {
                                isWholeWord = false;
                            }
                            
                            // Check character after
                            int endIndex = index + searchText.Length;
                            if (endIndex < textRun.Length && char.IsLetterOrDigit(textRun[endIndex]))
                            {
                                isWholeWord = false;
                            }

                            if (!isWholeWord)
                            {
                                // Move position forward and continue searching
                                position = position.GetPositionAtOffset(index + searchText.Length);
                                continue;
                            }
                        }

                        // Found a match
                        TextPointer? start = position.GetPositionAtOffset(index);
                        if (start == null)
                        {
                            position = position.GetNextContextPosition(LogicalDirection.Forward);
                            continue;
                        }

                        TextPointer? end = start.GetPositionAtOffset(searchText.Length);
                        if (end == null)
                        {
                            position = position.GetNextContextPosition(LogicalDirection.Forward);
                            continue;
                        }
                        
                        _richTextBox.Selection.Select(start, end);
                        _richTextBox.Focus();
                        
                        // Update current position for next search
                        _currentPosition = end;
                        
                        return true;
                    }
                }

                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }

            return false;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
