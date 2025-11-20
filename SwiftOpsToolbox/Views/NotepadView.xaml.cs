using DocumentFormat.OpenXml.Packaging;
using Markdig;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace SwiftOpsToolbox.Views
{
    public partial class NotepadView : System.Windows.Controls.UserControl
    {
        private MarkdownPipeline _markdownPipeline;
        private MarkdownWindow? _popoutWindow;

        public NotepadView()
        {
            InitializeComponent();

            _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            BtnNew.Click += BtnNew_Click;
            BtnOpen.Click += BtnOpen_Click;
            BtnSave.Click += BtnSave_Click;
            BtnSaveAs.Click += BtnSaveAs_Click;

            BtnBold.Checked += (s, e) => ToggleSelectionFormatting(System.Windows.FontWeights.Bold, null);
            BtnBold.Unchecked += (s, e) => ToggleSelectionFormatting(System.Windows.FontWeights.Normal, null);
            BtnItalic.Checked += (s, e) => ToggleSelectionFormatting(null, System.Windows.FontStyles.Italic);
            BtnItalic.Unchecked += (s, e) => ToggleSelectionFormatting(null, System.Windows.FontStyles.Normal);

            FontFamilyCombo.ItemsSource = System.Windows.Media.Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            FontFamilyCombo.SelectionChanged += FontFamilyCombo_SelectionChanged;

            FontSizeCombo.ItemsSource = new double[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 32 };
            FontSizeCombo.SelectionChanged += FontSizeCombo_SelectionChanged;

            BtnPreview.Checked += BtnPreview_Checked;
            BtnPreview.Unchecked += BtnPreview_Unchecked;

            BtnPopout.Click += BtnPopout_Click;

            Tabs.SelectionChanged += Tabs_SelectionChanged;

            // Ensure the initial XAML tab has the close button header
            EnsureInitialTabHeader();

            // Init with one tab focused
            EnsureTab(0);
        }

        private void EnsureInitialTabHeader()
        {
            if (Tabs.Items.Count > 0 && Tabs.Items[0] is TabItem tab)
            {
                // If header is already a StackPanel (from XAML), make sure the button.Tag points to the tab
                if (tab.Header is StackPanel sp)
                {
                    var btn = sp.Children.OfType<System.Windows.Controls.Button>().FirstOrDefault();
                    if (btn != null)
                    {
                        btn.Tag = tab;
                        btn.Click -= CloseTab_Click;
                        btn.Click += CloseTab_Click;
                    }
                    return;
                }

                // Replace header with StackPanel containing text and close button
                var headerPanel = CreateHeaderPanel("Untitled", tab);
                tab.Header = headerPanel;

                // Ensure the content is a RichTextBox; if not, wrap existing content
                if (!(tab.Content is System.Windows.Controls.RichTextBox))
                {
                    var existingContent = tab.Content as UIElement;
                    var rtb = new System.Windows.Controls.RichTextBox { AcceptsTab = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new System.Windows.Media.FontFamily("Consolas"), FontSize = 14 };
                    if (existingContent is System.Windows.Controls.RichTextBox oldRtb)
                    {
                        rtb = oldRtb;
                    }
                    else
                    {
                        if (existingContent is TextBlock tb)
                        {
                            rtb.Document.Blocks.Clear();
                            rtb.Document.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(tb.Text)));
                        }
                    }
                    tab.Content = rtb;
                }
            }
        }

        private StackPanel CreateHeaderPanel(string title, TabItem? tab)
        {
            var sp = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
            var tb = new TextBlock { Text = title, Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 8, 0) };
            var btn = new System.Windows.Controls.Button { Content = "?", Width = 18, Height = 18, Padding = new Thickness(0), Margin = new Thickness(0), ToolTip = "Close tab" };
            btn.Click += CloseTab_Click;
            if (tab != null)
            {
                btn.Tag = tab;
            }
            sp.Children.Add(tb);
            sp.Children.Add(btn);
            return sp;
        }

        private void SetTabHeaderText(TabItem tab, string title)
        {
            if (tab.Header is StackPanel sp && sp.Children.Count > 0 && sp.Children[0] is TextBlock tb)
            {
                tb.Text = title;
            }
            else
            {
                tab.Header = CreateHeaderPanel(title, tab);
            }
        }

        private TabItem CreateTab(string title, string? path, string? content)
        {
            var tab = new TabItem { Tag = path ?? string.Empty };
            tab.Header = CreateHeaderPanel(title, tab);
            var rtb = new System.Windows.Controls.RichTextBox { AcceptsTab = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new System.Windows.Media.FontFamily("Consolas"), FontSize = 14 };
            rtb.Document.Blocks.Clear();
            if (!string.IsNullOrEmpty(content))
            {
                rtb.Document.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(content)));
            }
            tab.Content = rtb;
            return tab;
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn)
            {
                if (btn.Tag is TabItem taggedTab)
                {
                    Tabs.Items.Remove(taggedTab);
                    return;
                }

                // fallback: try to locate TabItem via visual tree assumptions
                if (btn.Parent is StackPanel sp && sp.Parent is TabItem tab)
                {
                    Tabs.Items.Remove(tab);
                    return;
                }
            }

            // fallback: remove selected
            if (Tabs.SelectedItem is TabItem t) Tabs.Items.Remove(t);
        }

        private void BtnNew_Click(object? sender, RoutedEventArgs e)
        {
            var tab = CreateTab("Untitled", string.Empty, string.Empty);
            Tabs.Items.Add(tab);
            Tabs.SelectedItem = tab;
            var rtb = FindRtb(tab);
            rtb?.Focus();
        }

        private void BtnOpen_Click(object? sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Text files (*.txt)|*.txt|Word Documents (*.docx)|*.docx|All files (*.*)|*.*";
            if (ofd.ShowDialog() == true)
            {
                OpenFileInNewTab(ofd.FileName);
            }
        }

        private void OpenFileInNewTab(string path)
        {
            var content = File.ReadAllText(path);
            if (Path.GetExtension(path).ToLower() == ".docx")
            {
                // Read docx as plain text
                using var doc = WordprocessingDocument.Open(path, false);
                var body = doc.MainDocumentPart.Document.Body;
                content = string.Join("\n", body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().Select(p => p.InnerText));
            }

            var tab = CreateTab(System.IO.Path.GetFileName(path), path, content);
            Tabs.Items.Add(tab);
            Tabs.SelectedItem = tab;
            var rtb = FindRtb(tab);
            rtb?.Focus();
        }

        private void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            var tab = Tabs.SelectedItem as TabItem;
            if (tab == null) return;
            var path = tab.Tag as string;
            if (string.IsNullOrEmpty(path))
            {
                BtnSaveAs_Click(sender, e);
                return;
            }
            SaveTabToPath(tab, path);
        }

        private void BtnSaveAs_Click(object? sender, RoutedEventArgs e)
        {
            var tab = Tabs.SelectedItem as TabItem;
            if (tab == null) return;

            var sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Filter = "Text files (*.txt)|*.txt|Word Documents (*.docx)|*.docx|All files (*.*)|*.*";
            if (sfd.ShowDialog() == true)
            {
                SaveTabToPath(tab, sfd.FileName);
                SetTabHeaderText(tab, Path.GetFileName(sfd.FileName));
                tab.Tag = sfd.FileName;
            }
        }

        private void SaveTabToPath(TabItem tab, string path)
        {
            var rtb = FindRtb(tab);
            if (rtb == null) return;
            var text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
            if (Path.GetExtension(path).ToLower() == ".docx")
            {
                // Write simple docx with paragraphs
                using var mem = new MemoryStream();
                using (var doc = WordprocessingDocument.Create(mem, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                {
                    var main = doc.AddMainDocumentPart();
                    main.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    var body = main.Document.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Body());
                    foreach (var line in text.Split('\n'))
                    {
                        var p = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                        var r = new DocumentFormat.OpenXml.Wordprocessing.Run();
                        r.Append(new DocumentFormat.OpenXml.Wordprocessing.Text(line));
                        p.Append(r);
                        body.Append(p);
                    }
                    main.Document.Save();
                }
                File.WriteAllBytes(path, mem.ToArray());
            }
            else
            {
                File.WriteAllText(path, text);
            }
        }

        private void BtnPopout_Click(object? sender, RoutedEventArgs e)
        {
            var tmp = Path.Combine(Path.GetTempPath(), "gw_md_preview.html");
            RenderPreviewForActiveTab();

            if (_popoutWindow == null)
            {
                _popoutWindow = new MarkdownWindow();
                _popoutWindow.Closed += (s, ev) => _popoutWindow = null;
                _popoutWindow.Show();
            }

            _popoutWindow?.Navigate(tmp);
            _popoutWindow?.Activate();
        }

        private void BtnPreview_Unchecked(object? sender, RoutedEventArgs e)
        {
            PreviewBorder.Visibility = Visibility.Collapsed;
        }

        private void BtnPreview_Checked(object? sender, RoutedEventArgs e)
        {
            PreviewBorder.Visibility = Visibility.Visible;
            RenderPreviewForActiveTab();
        }

        private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (BtnPreview.IsChecked == true)
            {
                RenderPreviewForActiveTab();
            }
        }

        private void RenderPreviewForActiveTab()
        {
            var tab = Tabs.SelectedItem as TabItem;
            if (tab == null) return;
            var rtb = FindRtb(tab);
            if (rtb == null) return;
            var text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text ?? string.Empty;
            var html = Markdown.ToHtml(text, _markdownPipeline);
            var content = $"<html><head><meta charset=\"utf-8\"><style>body {{ background-color:#0F0F10; color:#FFFFFF; font-family:Segoe UI; padding:12px; }}</style></head><body>{html}</body></html>";
            var tmp = Path.Combine(Path.GetTempPath(), "gw_md_preview.html");
            File.WriteAllText(tmp, content);
            MarkdownPreview.Navigate(tmp);
        }

        private void EnsureTab(int index)
        {
            if (Tabs.Items.Count <= index) return;
            var tab = Tabs.Items[index] as TabItem;
            if (tab is not null)
            {
                var rtb = FindRtb(tab);
                if (rtb is not null)
                {
                    rtb.Focus();
                }
            }
        }

        private System.Windows.Controls.RichTextBox? FindRtb(TabItem tab)
        {
            return tab?.Content as System.Windows.Controls.RichTextBox;
        }

        // public OpenFile method
        public void OpenFile(string path)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OpenFile(path));
                return;
            }

            OpenFileInNewTab(path);
        }

        private void ToggleSelectionFormatting(System.Windows.FontWeight? weight, System.Windows.FontStyle? style)
        {
            var tab = Tabs.SelectedItem as TabItem;
            var rtb = FindRtb(tab);
            if (rtb == null) return;

            if (weight.HasValue)
            {
                rtb.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, weight.Value);
            }
            if (style.HasValue)
            {
                rtb.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, style.Value);
            }
            rtb.Focus();
        }

        private void FontFamilyCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var tab = Tabs.SelectedItem as TabItem;
            var rtb = FindRtb(tab);
            if (rtb == null) return;
            if (FontFamilyCombo.SelectedItem is System.Windows.Media.FontFamily ff)
            {
                rtb.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, ff);
            }
        }

        private void FontSizeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var tab = Tabs.SelectedItem as TabItem;
            var rtb = FindRtb(tab);
            if (rtb == null) return;
            if (double.TryParse(FontSizeCombo.Text, out var size))
            {
                rtb.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, size);
            }
        }
    }
}