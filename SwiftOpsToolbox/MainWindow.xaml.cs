using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Collections.Specialized;
using System.IO;
using Markdig;
using MessageBox = System.Windows.MessageBox;

namespace SwiftOpsToolbox
{
    public partial class MainWindow : MetroWindow
    {
        private DateTime _displayMonth = DateTime.Today;

        public MainWindow()
        {
            InitializeComponent();
            // set DataContext if not set
            if (DataContext == null) DataContext = new ViewModels.MainViewModel();

            // If VM provides CalendarEvents, subscribe to updates to refresh grid automatically
            if (DataContext is ViewModels.MainViewModel vm && vm.CalendarEvents is System.Collections.ObjectModel.ObservableCollection<Models.CalendarEvent> coll)
            {
                coll.CollectionChanged += CalendarEvents_CollectionChanged;
            }

            // wire header add event button if present
            if (FindName("BtnAddEventHeader") is System.Windows.Controls.Button btn)
            {
                btn.Click += (s, e) => BtnAddEventHeader_Click(s, e);
            }

            if (FindName("BtnAddEvent") is System.Windows.Controls.Button btn2)
            {
                btn2.Click += (s, e) => BtnAddEventHeader_Click(s, e);
            }

            // wire clipboard list double click
            if (FindName("ClipboardList") is System.Windows.Controls.ListBox lb)
            {
                lb.MouseDoubleClick += ClipboardList_MouseDoubleClick;
            }

            // wire month navigation
            if (FindName("BtnPrevMonth") is System.Windows.Controls.Button prev)
            {
                prev.Click += (s, e) => { _displayMonth = _displayMonth.AddMonths(-1); RefreshMonthGrid(); };
            }
            if (FindName("BtnNextMonth") is System.Windows.Controls.Button next)
            {
                next.Click += (s, e) => { _displayMonth = _displayMonth.AddMonths(1); RefreshMonthGrid(); };
            }

            Loaded += (s, e) => RefreshMonthGrid();

            // Bind F1/Help command to open user documentation
            CommandBindings.Add(new CommandBinding(System.Windows.Input.ApplicationCommands.Help, (s, e) => OpenUserDocumentation_Click(s, (RoutedEventArgs?)e.Parameter)));
        }

        private void CalendarEvents_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // ensure UI refresh on main thread
            Dispatcher.Invoke(() => RefreshMonthGrid());
        }

        private void RefreshMonthGrid()
        {
            if (FindName("MonthGrid") is Views.MonthGridView mg && DataContext is ViewModels.MainViewModel vm)
            {
                mg.SetMonth(_displayMonth, vm.CalendarEvents.ToArray());
            }
        }

        private void ClipboardList_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ListBox lb && lb.SelectedItem is SwiftOpsToolbox.Models.ClipboardItem item)
            {
                try
                {
                    System.Windows.Clipboard.SetText(item.Text);
                }
                catch { }
            }
        }

        private void BtnAddEventHeader_Click(object? sender, RoutedEventArgs e)
        {
            var dlg = new Views.AddEventDialog();
            dlg.Owner = this;
            var res = dlg.ShowDialog();
            if (res == true && dlg.Result != null)
            {
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.CalendarEvents.Add(new Models.CalendarEvent
                    {
                        StartDate = dlg.Result.StartDate,
                        EndDate = dlg.Result.EndDate,
                        Time = dlg.Result.Time,
                        Title = dlg.Result.Title,
                        Description = dlg.Result.Description,
                        Color = dlg.Result.Color
                    });

                    vm.RefreshSelectedDateEvents();
                    vm.RefreshUpcomingEvents();
                    RefreshMonthGrid();
                }
            }
        }

        private void BtnAddEvent_Click(object? sender, RoutedEventArgs e)
        {
            // For now open Notepad mode to allow adding event notes or future: open event modal
            BtnNotepad_Click(sender, new RoutedEventArgs());
        }

        // SelectionChanged stubs
        private void TierComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm && sender is System.Windows.Controls.ComboBox cb && cb.SelectedValue is string s)
            {
                vm.CurrentTierName = s;
            }
        }

        private void ThemeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm && sender is System.Windows.Controls.ComboBox cb && cb.SelectedValue is string s)
            {
                vm.Theme = s;
            }
        }

        private void DefaultViewComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm && sender is System.Windows.Controls.ComboBox cb && cb.SelectedValue is string s)
            {
                vm.DefaultView = s;
            }
        }

        // CheckBox changed handlers
        private void StartOnCalendarCheckBox_Changed(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm && sender is System.Windows.Controls.CheckBox checkBox)
            {
                vm.StartOnCalendar = checkBox.IsChecked ?? false;
            }
        }

        private void Use24HourCheckBox_Changed(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm && sender is System.Windows.Controls.CheckBox checkBox)
            {
                vm.Use24Hour = checkBox.IsChecked ?? false;
            }
        }

        // Mouse event handlers used by calendar UI
        private void DaySlot_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            // Placeholder - actual implementation lives in ViewModel
        }

        private void WeekEvent_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            // Placeholder - actual implementation lives in ViewModel
        }

        // Helper: show or hide the agenda (right card) and adjust column widths
        private void SetAgendaVisible(bool visible)
        {
            if (FindName("RightCardBorder") is System.Windows.Controls.Border rc)
            {
                rc.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            }

            var leftCol = FindName("LeftColumn") as ColumnDefinition;
            var rightCol = FindName("RightColumn") as ColumnDefinition;
            if (leftCol != null && rightCol != null)
            {
                if (visible)
                {
                    leftCol.Width = new GridLength(3, GridUnitType.Star);
                    rightCol.Width = new GridLength(2, GridUnitType.Star);
                }
                else
                {
                    leftCol.Width = new GridLength(1, GridUnitType.Star);
                    rightCol.Width = new GridLength(0, GridUnitType.Pixel);
                }
            }
        }

        // Left-nav click handlers to toggle mode panels
        private void BtnTasks_Click(object sender, RoutedEventArgs e)
        {
            HomePanel.Visibility = Visibility.Visible;
            CalendarModePanel.Visibility = Visibility.Collapsed;
            TasksModePanel.Visibility = Visibility.Collapsed;
            ClipboardModePanel.Visibility = Visibility.Collapsed;
            NotepadModePanel.Visibility = Visibility.Collapsed;
            SearchModePanel.Visibility = Visibility.Collapsed;
            SettingsModePanel.Visibility = Visibility.Collapsed;
            // show right card when on home
            SetAgendaVisible(true);
        }

        private void BtnCalendar_Click(object sender, RoutedEventArgs e)
        {
            HomePanel.Visibility = Visibility.Collapsed;
            CalendarModePanel.Visibility = Visibility.Visible;
            TasksModePanel.Visibility = Visibility.Collapsed;
            ClipboardModePanel.Visibility = Visibility.Collapsed;
            NotepadModePanel.Visibility = Visibility.Collapsed;
            SearchModePanel.Visibility = Visibility.Collapsed;
            SettingsModePanel.Visibility = Visibility.Collapsed;
            // show right card on calendar as well
            SetAgendaVisible(true);
        }

        private void BtnClipboard_Click(object sender, RoutedEventArgs e)
        {
            HomePanel.Visibility = Visibility.Collapsed;
            CalendarModePanel.Visibility = Visibility.Collapsed;
            TasksModePanel.Visibility = Visibility.Collapsed;
            ClipboardModePanel.Visibility = Visibility.Visible;
            NotepadModePanel.Visibility = Visibility.Collapsed;
            SearchModePanel.Visibility = Visibility.Collapsed;
            SettingsModePanel.Visibility = Visibility.Collapsed;
            // hide agenda on non-home/calendar pages
            SetAgendaVisible(false);
        }

        private void BtnNotepad_Click(object sender, RoutedEventArgs e)
        {
            HomePanel.Visibility = Visibility.Collapsed;
            CalendarModePanel.Visibility = Visibility.Collapsed;
            TasksModePanel.Visibility = Visibility.Collapsed;
            ClipboardModePanel.Visibility = Visibility.Collapsed;
            NotepadModePanel.Visibility = Visibility.Visible;
            SearchModePanel.Visibility = Visibility.Collapsed;
            SettingsModePanel.Visibility = Visibility.Collapsed;
            SetAgendaVisible(false);
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            HomePanel.Visibility = Visibility.Collapsed;
            CalendarModePanel.Visibility = Visibility.Collapsed;
            TasksModePanel.Visibility = Visibility.Collapsed;
            ClipboardModePanel.Visibility = Visibility.Collapsed;
            NotepadModePanel.Visibility = Visibility.Collapsed;
            SearchModePanel.Visibility = Visibility.Visible;
            SettingsModePanel.Visibility = Visibility.Collapsed;
            SetAgendaVisible(false);
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            HomePanel.Visibility = Visibility.Collapsed;
            CalendarModePanel.Visibility = Visibility.Collapsed;
            TasksModePanel.Visibility = Visibility.Collapsed;
            ClipboardModePanel.Visibility = Visibility.Collapsed;
            NotepadModePanel.Visibility = Visibility.Collapsed;
            SearchModePanel.Visibility = Visibility.Collapsed;
            SettingsModePanel.Visibility = Visibility.Visible;
            SetAgendaVisible(false);
        }

        // Open the user documentation (docs/USER_DOCUMENTATION.md) in the MarkdownWindow
        private void OpenUserDocumentation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mdPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "docs", "USER_DOCUMENTATION.md");
                mdPath = System.IO.Path.GetFullPath(mdPath);
                if (!File.Exists(mdPath))
                {
                    // try repo-relative path (when running from bin folder)
                    var alt = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", "USER_DOCUMENTATION.md");
                    if (File.Exists(alt)) mdPath = alt;
                }

                if (!File.Exists(mdPath))
                {
                    MessageBox.Show(this, $"Documentation file not found:\n{mdPath}", "Not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Convert markdown to HTML using Markdig (same pipeline used elsewhere)
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                var md = File.ReadAllText(mdPath);
                var html = Markdig.Markdown.ToHtml(md, pipeline);

                var content = $"<html><head><meta charset=\"utf-8\"><style>body{{background-color:#0F0F10;color:#FFFFFF;font-family:Segoe UI;padding:12px;}}img{{max-width:100%;height:auto;}}</style></head><body>{html}</body></html>";
                var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "swiftops_userdoc_preview.html");
                File.WriteAllText(tmp, content);

                var win = new Views.MarkdownWindow();
                win.Owner = this;
                win.Show();
                win.Navigate(tmp);
                win.Activate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to open documentation: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}