using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

// Need WinForms for Screen
using System.Drawing;
using System.Windows.Interop;

using SwiftOpsToolbox.Views;
using SwiftOpsToolbox.Models;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace SwiftOpsToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            // If not running elevated, offer to restart elevated so indexer can access protected areas
            var args = System.Environment.GetCommandLineArgs();
            bool alreadyElevatedStart = args.Any(a => a == "--elevated");
            if (!IsRunningAsAdmin() && !alreadyElevatedStart)
            {
                var result = System.Windows.MessageBox.Show("This app can index the entire file system if run with administrator privileges. Restart elevated now?", "Require elevation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = Process.GetCurrentProcess().MainModule.FileName,
                            UseShellExecute = true,
                            Verb = "runas",
                            Arguments = "--elevated"
                        };
                        Process.Start(psi);
                        System.Windows.Application.Current.Shutdown();
                        return;
                    }
                    catch
                    {
                        // user cancelled or failed to elevate — continue without elevation
                    }
                }
            }

            InitializeComponent();
            DataContext = new ViewModels.MainViewModel();

            // Apply initial theme and subscribe to changes
            if (DataContext is ViewModels.MainViewModel vm)
            {
                ApplyTheme(vm.Theme);
                
                // Subscribe to PropertyChanged to apply theme instantly when Theme property changes
                vm.PropertyChanged += Vm_PropertyChanged;
                
                // Apply settings only when user saves
                vm.SettingsSaved += (s, e) =>
                {
                    // if configured to start on calendar, switch to calendar view
                    if (vm.StartOnCalendar)
                    {
                        ShowView("Calendar");
                    }

                    // apply default calendar subview
                    if (!string.IsNullOrEmpty(vm.DefaultView))
                    {
                        ShowCalendarViewMode(vm.DefaultView);
                    }
                };
            }

            // Map sidebar buttons to mode panels
            BtnTasks.Click += (s, e) => ShowView("Home");      // Home -> Home panel
            BtnCalendar.Click += (s, e) => ShowView("Calendar");
            BtnClipboard.Click += (s, e) => ShowView("Clipboard");
            BtnSearch.Click += (s, e) => ShowView("Search");
            var notepadBtn = this.FindName("BtnNotepad") as System.Windows.Controls.Button;
            if (notepadBtn != null) notepadBtn.Click += (s, e) => ShowView("Notepad");
            BtnSettings.Click += (s, e) => ShowView("Settings");     // Settings button opens Settings mode

            // Place on a dedicated monitor and maximize
            PlaceOnDedicatedMonitor();

            // Default view
            ShowView("Calendar");

            // Wire up calendar selection changed
            Loaded += MainWindow_Loaded;

            // Wire up event icon buttons
            var addBtn = this.FindName("BtnAddEvent") as System.Windows.Controls.Button;
            var editBtn = this.FindName("BtnEditEvent") as System.Windows.Controls.Button;
            var delBtn = this.FindName("BtnDeleteEvent") as System.Windows.Controls.Button;
            var lb = this.FindName("LbEvents") as System.Windows.Controls.ListBox;

            if (addBtn != null) addBtn.Click += AddBtn_Click;
            if (editBtn != null) editBtn.Click += EditBtn_Click;
            if (delBtn != null) delBtn.Click += DelBtn_Click;
            if (lb != null) lb.MouseDoubleClick += Lb_MouseDoubleClick;

            // Hook view mode buttons to change calendar subviews
            var threeBtn = this.FindName("ThreeMonthViewBtn") as System.Windows.Controls.Button;
            var monthBtn = this.FindName("MonthViewBtn") as System.Windows.Controls.Button;
            var weekBtn = this.FindName("WeekViewBtn") as System.Windows.Controls.Button;
            var dayBtn = this.FindName("DayViewBtn") as System.Windows.Controls.Button;
            if (threeBtn != null) threeBtn.Click += (s, e) => ShowCalendarViewMode("ThreeMonth");
            if (monthBtn != null) monthBtn.Click += (s, e) => ShowCalendarViewMode("SingleMonth");
            if (weekBtn != null) weekBtn.Click += (s, e) => ShowCalendarViewMode("Week");
            if (dayBtn != null) dayBtn.Click += (s, e) => ShowCalendarViewMode("Day");
            
            // Subscribe to Closed event for cleanup
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            // Unsubscribe from PropertyChanged to prevent memory leak
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.PropertyChanged -= Vm_PropertyChanged;
            }
        }

        private static bool IsRunningAsAdmin()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ViewModels.MainViewModel vm && e.PropertyName == "Theme")
            {
                ApplyTheme(vm.Theme);
            }
        }

        private void ApplyTheme(string theme)
        {
            try
            {
                var isLight = (theme ?? "Dark").Equals("Light", System.StringComparison.OrdinalIgnoreCase);
                var isElectricBlue = (theme ?? "Dark").Equals("Electric Blue", System.StringComparison.OrdinalIgnoreCase);
                var isVibrantOrange = (theme ?? "Dark").Equals("Vibrant Orange", System.StringComparison.OrdinalIgnoreCase);

                // target colors
                System.Windows.Media.Color targetSurface, targetPanel, targetAccent, targetMuted;

                if (isElectricBlue)
                {
                    // Electric Blue theme - uses Electric Blue as primary accent with dark backgrounds
                    targetSurface = System.Windows.Media.Color.FromRgb(0x0F, 0x0F, 0x10);
                    targetPanel = System.Windows.Media.Color.FromRgb(0x16, 0x16, 0x16);
                    targetAccent = System.Windows.Media.Color.FromRgb(0x00, 0x7B, 0xFF); // Electric Blue
                    targetMuted = System.Windows.Media.Color.FromRgb(0xA6, 0xA6, 0xA6);
                }
                else if (isVibrantOrange)
                {
                    // Vibrant Orange theme - uses Vibrant Orange as primary accent with dark backgrounds
                    targetSurface = System.Windows.Media.Color.FromRgb(0x0F, 0x0F, 0x10);
                    targetPanel = System.Windows.Media.Color.FromRgb(0x16, 0x16, 0x16);
                    targetAccent = System.Windows.Media.Color.FromRgb(0xFF, 0x6F, 0x00); // Vibrant Orange
                    targetMuted = System.Windows.Media.Color.FromRgb(0xA6, 0xA6, 0xA6);
                }
                else if (isLight)
                {
                    // Light theme
                    targetSurface = System.Windows.Media.Color.FromRgb(0xF3, 0xF3, 0xF3);
                    targetPanel = System.Windows.Media.Color.FromRgb(0xFF, 0xFF, 0xFF);
                    targetAccent = System.Windows.Media.Color.FromRgb(0x00, 0x7B, 0xFF); // Electric Blue
                    targetMuted = System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66);
                }
                else
                {
                    // Dark theme (default)
                    targetSurface = System.Windows.Media.Color.FromRgb(0x0F, 0x0F, 0x10);
                    targetPanel = System.Windows.Media.Color.FromRgb(0x16, 0x16, 0x16);
                    targetAccent = System.Windows.Media.Color.FromRgb(0x00, 0x7B, 0xFF); // Electric Blue
                    targetMuted = System.Windows.Media.Color.FromRgb(0xA6, 0xA6, 0xA6);
                }

                // prefer updating the Window resources so XAML StaticResource lookups pick up changes
                AnimateResourceBrushTo(this.Resources, "SurfaceBrush", targetSurface);
                AnimateResourceBrushTo(this.Resources, "PanelBrush", targetPanel);
                AnimateResourceBrushTo(this.Resources, "AccentBrush", targetAccent);
                AnimateResourceBrushTo(this.Resources, "MutedText", targetMuted);

                // also ensure Application resources are updated so other windows/new controls pick it up
                AnimateResourceBrushTo(System.Windows.Application.Current.Resources, "SurfaceBrush", targetSurface);
                AnimateResourceBrushTo(System.Windows.Application.Current.Resources, "PanelBrush", targetPanel);
                AnimateResourceBrushTo(System.Windows.Application.Current.Resources, "AccentBrush", targetAccent);
                AnimateResourceBrushTo(System.Windows.Application.Current.Resources, "MutedText", targetMuted);

                // also update window background if it's a SolidColorBrush
                if (this.Background is System.Windows.Media.SolidColorBrush bgBrush)
                {
                    var anim = new ColorAnimation(targetSurface, new Duration(TimeSpan.FromMilliseconds(300))) { EasingFunction = new QuadraticEase() };
                    bgBrush.BeginAnimation(System.Windows.Media.SolidColorBrush.ColorProperty, anim);
                }
                else
                {
                    // set background to the SurfaceBrush defined in window resources
                    if (this.Resources.Contains("SurfaceBrush") && this.Resources["SurfaceBrush"] is System.Windows.Media.Brush surf)
                    {
                        this.Background = surf;
                    }
                    else if (System.Windows.Application.Current.Resources.Contains("SurfaceBrush") && System.Windows.Application.Current.Resources["SurfaceBrush"] is System.Windows.Media.Brush surfApp)
                    {
                        this.Background = surfApp;
                    }
                }
            }
            catch
            {
                // ignore theme errors
            }
        }

        private void AnimateResourceBrushTo(System.Windows.ResourceDictionary targetDict, string key, System.Windows.Media.Color targetColor)
        {
            try
            {
                if (targetDict.Contains(key) && targetDict[key] is System.Windows.Media.SolidColorBrush existingBrush)
                {
                    var anim = new ColorAnimation(targetColor, new Duration(TimeSpan.FromMilliseconds(300))) { EasingFunction = new QuadraticEase() };
                    existingBrush.BeginAnimation(System.Windows.Media.SolidColorBrush.ColorProperty, anim);
                }
                else
                {
                    targetDict[key] = new System.Windows.Media.SolidColorBrush(targetColor);
                }
            }
            catch
            {
                // ignore
            }
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            var calSingle = this.FindName("SingleCal") as Calendar;
            var cal1 = this.FindName("Cal1") as Calendar;
            var cal2 = this.FindName("Cal2") as Calendar;
            var cal3 = this.FindName("Cal3") as Calendar;
            if (calSingle != null) calSingle.SelectedDatesChanged += Calendar_SelectedDatesChanged;
            if (cal1 != null) cal1.SelectedDatesChanged += Calendar_SelectedDatesChanged;
            if (cal2 != null) cal2.SelectedDatesChanged += Calendar_SelectedDatesChanged;
            if (cal3 != null) cal3.SelectedDatesChanged += Calendar_SelectedDatesChanged;

            RenderPlanners();

            // start file indexing after UI has rendered
            if (DataContext is ViewModels.MainViewModel vm)
            {
                // defer work to background dispatcher priority so initial UI renders first
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    try
                    {
                        var args = System.Environment.GetCommandLineArgs();
                        var elevatedFlag = args.Any(a => a == "--elevated");
                        if (IsRunningAsAdmin() || elevatedFlag)
                        {
                            // run full-drive indexing when elevated (indexes all ready drives)
                            if (vm.StartIndexingCommand.CanExecute(null)) vm.StartIndexingCommand.Execute(null);
                        }
                        else
                        {
                            // otherwise index only configured roots
                            if (vm.StartIndexRootsCommand.CanExecute(null)) vm.StartIndexRootsCommand.Execute(null);
                        }
                    }
                    catch { }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void RenderPlanners()
        {
            var vm = DataContext as ViewModels.MainViewModel;
            if (vm == null) return;

            // Day slots 6:00 to 22:00
            var slots = new List<object>();
            for (int h = 6; h <= 22; h++)
            {
                var slotTime = h.ToString("D2") + ":00";
                var ev = vm.CalendarEvents.FirstOrDefault(e => e.Date.Date == vm.SelectedDate.Date && e.Time == slotTime);
                slots.Add(new { SlotText = slotTime, EventText = ev != null ? ev.Title : string.Empty });
            }
            DaySlots.ItemsSource = slots;

            // Week columns
            var cols = new List<object>();
            var d = vm.SelectedDate;
            int diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = d.AddDays(-diff).Date;
            for (int i = 0; i < 7; i++)
            {
                var day = weekStart.AddDays(i);
                var events = vm.CalendarEvents.Where(ev => ev.Date.Date == day.Date).OrderBy(ev => ev.Time).ToList();
                var dayModel = new
                {
                    DayHeader = day.ToString("ddd d"),
                    Events = events.Select(ev => new { ev.Time, ev.Title, ev.Description }).ToList()
                };
                cols.Add(dayModel);
            }
            WeekColumns.ItemsSource = cols;
        }

        private void Lb_MouseDoubleClick(object? sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditSelectedEvent();
        }

        private void AddBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not ViewModels.MainViewModel vm) return;
            var ev = new CalendarEvent { Date = vm.SelectedDate };
            var dlg = new EventEditor(ev) { Owner = this };
            if (dlg.ShowDialog() == true && dlg.Event != null)
            {
                vm.AddCalendarEvent(dlg.Event);
                vm.RefreshSelectedDateEvents();
                vm.RefreshSelectedWeekEvents();
                RenderPlanners();
            }
        }

        private void EditBtn_Click(object? sender, RoutedEventArgs e)
        {
            EditSelectedEvent();
        }

        private void EditSelectedEvent()
        {
            if (DataContext is not ViewModels.MainViewModel vm) return;
            var lb = this.FindName("LbEvents") as System.Windows.Controls.ListBox;
            if (lb == null) return;
            if (lb.SelectedItem is CalendarEvent ev)
            {
                var copy = new CalendarEvent { Date = ev.Date, Time = ev.Time, Title = ev.Title, Description = ev.Description };
                var dlg = new EventEditor(copy) { Owner = this };
                if (dlg.ShowDialog() == true && dlg.Event != null)
                {
                    ev.Date = dlg.Event.Date;
                    ev.Time = dlg.Event.Time;
                    ev.Title = dlg.Event.Title;
                    ev.Description = dlg.Event.Description;
                    vm.SaveCalendarEvents();
                    vm.RefreshSelectedDateEvents();
                    vm.RefreshSelectedWeekEvents();
                    RenderPlanners();
                }
            }
        }

        private void DelBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not ViewModels.MainViewModel vm) return;
            var lb = this.FindName("LbEvents") as System.Windows.Controls.ListBox;
            if (lb == null) return;
            if (lb.SelectedItem is CalendarEvent ev)
            {
                vm.RemoveCalendarEvent(ev);
                RenderPlanners();
            }
        }

        private void ShowCalendarViewMode(string mode)
        {
            var single = this.FindName("SingleMonthPanel") as FrameworkElement;
            var month = this.FindName("MonthViewPanel") as FrameworkElement; // stacked 3-month panel
            var week = this.FindName("WeekViewPanel") as FrameworkElement;
            var day = this.FindName("DayViewPanel") as FrameworkElement;

            switch (mode)
            {
                case "ThreeMonth":
                    // show stacked 3-month calendars (MonthViewPanel)
                    if (month != null) month.Visibility = Visibility.Visible;
                    if (single != null) single.Visibility = Visibility.Collapsed;
                    if (week != null) week.Visibility = Visibility.Collapsed;
                    if (day != null) day.Visibility = Visibility.Collapsed;
                    break;
                case "SingleMonth":
                case "Month":
                    // show single month calendar
                    if (month != null) month.Visibility = Visibility.Collapsed;
                    if (single != null) single.Visibility = Visibility.Visible;
                    if (week != null) week.Visibility = Visibility.Collapsed;
                    if (day != null) day.Visibility = Visibility.Collapsed;
                    break;
                case "Week":
                    if (month != null) month.Visibility = Visibility.Collapsed;
                    if (week != null) week.Visibility = Visibility.Visible;
                    if (single != null) single.Visibility = Visibility.Collapsed;
                    if (day != null) day.Visibility = Visibility.Collapsed;
                    break;
                case "Day":
                    if (month != null) month.Visibility = Visibility.Collapsed;
                    if (week != null) week.Visibility = Visibility.Collapsed;
                    if (single != null) single.Visibility = Visibility.Collapsed;
                    if (day != null) day.Visibility = Visibility.Visible;
                    break;
            }
            RenderPlanners();
        }

        private void ShowView(string view)
        {
            var calendarMode = this.FindName("CalendarModePanel") as FrameworkElement;
            var tasksMode = this.FindName("TasksModePanel") as FrameworkElement;
            var clipboardMode = this.FindName("ClipboardModePanel") as FrameworkElement;
            var notepadMode = this.FindName("NotepadModePanel") as FrameworkElement;
            var settingsMode = this.FindName("SettingsModePanel") as FrameworkElement;
            var searchMode = this.FindName("SearchModePanel") as FrameworkElement;
            var homePanel = this.FindName("HomePanel") as FrameworkElement;
            var agenda = this.FindName("AgendaPanel") as FrameworkElement;

            // column defs
            var leftCol = this.FindName("LeftColumn") as ColumnDefinition;
            var rightCol = this.FindName("RightColumn") as ColumnDefinition;

            switch (view)
            {
                case "Home":
                    if (homePanel != null) homePanel.Visibility = Visibility.Visible;
                    if (calendarMode != null) calendarMode.Visibility = Visibility.Collapsed;
                    if (tasksMode != null) tasksMode.Visibility = Visibility.Collapsed;
                    if (clipboardMode != null) clipboardMode.Visibility = Visibility.Collapsed;
                    if (notepadMode != null) notepadMode.Visibility = Visibility.Collapsed;
                    if (searchMode != null) searchMode.Visibility = Visibility.Collapsed;
                    if (settingsMode != null) settingsMode.Visibility = Visibility.Collapsed;
                    if (agenda != null) agenda.Visibility = Visibility.Collapsed;
                    // expand left to full width
                    if (leftCol != null && rightCol != null) { leftCol.Width = new GridLength(1, GridUnitType.Star); rightCol.Width = new GridLength(0); }
                    break;
                case "Calendar":
                    if (calendarMode != null) calendarMode.Visibility = Visibility.Visible;
                    if (homePanel != null) homePanel.Visibility = Visibility.Collapsed;
                    if (tasksMode != null) tasksMode.Visibility = Visibility.Collapsed;
                    if (clipboardMode != null) clipboardMode.Visibility = Visibility.Collapsed;
                    if (notepadMode != null) notepadMode.Visibility = Visibility.Collapsed;
                    if (searchMode != null) searchMode.Visibility = Visibility.Collapsed;
                    if (settingsMode != null) settingsMode.Visibility = Visibility.Collapsed;
                    if (agenda != null) agenda.Visibility = Visibility.Visible;
                    // restore split 3*:2*
                    if (leftCol != null && rightCol != null) { leftCol.Width = new GridLength(3, GridUnitType.Star); rightCol.Width = new GridLength(2, GridUnitType.Star); }
                    break;
                case "Tasks":
                    if (calendarMode != null) calendarMode.Visibility = Visibility.Collapsed;
                    if (tasksMode != null) tasksMode.Visibility = Visibility.Visible;
                    if (clipboardMode != null) clipboardMode.Visibility = Visibility.Collapsed;
                    if (notepadMode != null) notepadMode.Visibility = Visibility.Collapsed;
                    if (searchMode != null) searchMode.Visibility = Visibility.Collapsed;
                    if (settingsMode != null) settingsMode.Visibility = Visibility.Collapsed;
                    if (agenda != null) agenda.Visibility = Visibility.Collapsed;
                    // expand left
                    if (leftCol != null && rightCol != null) { leftCol.Width = new GridLength(1, GridUnitType.Star); rightCol.Width = new GridLength(0); }
                    break;
                case "Clipboard":
                    if (calendarMode != null) calendarMode.Visibility = Visibility.Collapsed;
                    if (tasksMode != null) tasksMode.Visibility = Visibility.Collapsed;
                    if (clipboardMode != null) clipboardMode.Visibility = Visibility.Visible;
                    if (notepadMode != null) notepadMode.Visibility = Visibility.Collapsed;
                    if (searchMode != null) searchMode.Visibility = Visibility.Collapsed;
                    if (settingsMode != null) settingsMode.Visibility = Visibility.Collapsed;
                    if (agenda != null) agenda.Visibility = Visibility.Collapsed;
                    // expand left (remove agenda)
                    if (leftCol != null && rightCol != null) { leftCol.Width = new GridLength(1, GridUnitType.Star); rightCol.Width = new GridLength(0); }
                    break;
                case "Notepad":
                    if (calendarMode != null) calendarMode.Visibility = Visibility.Collapsed;
                    if (tasksMode != null) tasksMode.Visibility = Visibility.Collapsed;
                    if (clipboardMode != null) clipboardMode.Visibility = Visibility.Collapsed;
                    if (notepadMode != null) notepadMode.Visibility = Visibility.Visible;
                    if (searchMode != null) searchMode.Visibility = Visibility.Collapsed;
                    if (settingsMode != null) settingsMode.Visibility = Visibility.Collapsed;
                    if (agenda != null) agenda.Visibility = Visibility.Collapsed;
                    // expand left (remove agenda)
                    if (leftCol != null && rightCol != null) { leftCol.Width = new GridLength(1, GridUnitType.Star); rightCol.Width = new GridLength(0); }
                    break;
                case "Settings":
                    if (calendarMode != null) calendarMode.Visibility = Visibility.Collapsed;
                    if (tasksMode != null) tasksMode.Visibility = Visibility.Collapsed;
                    if (clipboardMode != null) clipboardMode.Visibility = Visibility.Collapsed;
                    if (notepadMode != null) notepadMode.Visibility = Visibility.Collapsed;
                    if (searchMode != null) searchMode.Visibility = Visibility.Collapsed;
                    if (settingsMode != null) settingsMode.Visibility = Visibility.Visible;
                    if (agenda != null) agenda.Visibility = Visibility.Collapsed;
                    // expand left (settings take full area)
                    if (leftCol != null && rightCol != null) { leftCol.Width = new GridLength(1, GridUnitType.Star); rightCol.Width = new GridLength(0); }
                    break;
                case "SettingsOld":
                    // legacy fallback (shouldn't be used)
                    if (calendarMode != null) calendarMode.Visibility = Visibility.Visible;
                    if (tasksMode != null) tasksMode.Visibility = Visibility.Collapsed;
                    if (clipboardMode != null) clipboardMode.Visibility = Visibility.Collapsed;
                    if (notepadMode != null) notepadMode.Visibility = Visibility.Collapsed;
                    if (searchMode != null) searchMode.Visibility = Visibility.Collapsed;
                    if (agenda != null) agenda.Visibility = Visibility.Collapsed;
                    if (leftCol != null && rightCol != null) { leftCol.Width = new GridLength(3, GridUnitType.Star); rightCol.Width = new GridLength(2, GridUnitType.Star); }
                    break;
                case "Search":
                    if (homePanel != null) homePanel.Visibility = Visibility.Collapsed;
                    if (calendarMode != null) calendarMode.Visibility = Visibility.Collapsed;
                    if (tasksMode != null) tasksMode.Visibility = Visibility.Collapsed;
                    if (clipboardMode != null) clipboardMode.Visibility = Visibility.Collapsed;
                    if (notepadMode != null) notepadMode.Visibility = Visibility.Collapsed;
                    if (searchMode != null) searchMode.Visibility = Visibility.Visible;
                    if (settingsMode != null) settingsMode.Visibility = Visibility.Collapsed;
                    if (agenda != null) agenda.Visibility = Visibility.Collapsed;
                    if (leftCol != null && rightCol != null) { leftCol.Width = new GridLength(1, GridUnitType.Star); rightCol.Width = new GridLength(0); }
                    break;
            }
        }

        private void Calendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            var cal = sender as Calendar;
            if (cal?.SelectedDate != null)
            {
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.SelectedDate = cal.SelectedDate.Value;

                    // Update week/day when date changes
                    vm.RefreshSelectedWeekEvents();
                    vm.RefreshSelectedDateEvents();

                    // Switch the calendar subview based on ViewMode
                    if (vm.ViewMode == "Day")
                    {
                        ShowCalendarViewMode("Day");
                    }
                    else if (vm.ViewMode == "Week")
                    {
                        ShowCalendarViewMode("Week");
                    }
                    else
                    {
                        ShowCalendarViewMode("Month");
                    }
                }
            }
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e, bool unused)
        {
            // placeholder overload
        }

        private void PlaceOnDedicatedMonitor()
        {
            // If multiple screens exist, place on the last (external) screen, otherwise use primary
            var screens = System.Windows.Forms.Screen.AllScreens;
            var target = screens.Length > 1 ? screens[screens.Length - 1] : System.Windows.Forms.Screen.PrimaryScreen;

            // Convert screen bounds to WPF units
            var bounds = target.Bounds;

            // Set window position and size
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = bounds.X;
            this.Top = bounds.Y;
            this.Width = bounds.Width;
            this.Height = bounds.Height;

            this.WindowState = WindowState.Maximized;
        }

        private void DaySlot_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border b && b.DataContext != null)
            {
                var slotText = (string)b.DataContext.GetType().GetProperty("SlotText")?.GetValue(b.DataContext);
                if (!string.IsNullOrEmpty(slotText) && DataContext is ViewModels.MainViewModel vm)
                {
                    var ev = new CalendarEvent { Date = vm.SelectedDate, Time = slotText };
                    var dlg = new EventEditor(ev) { Owner = this };
                    if (dlg.ShowDialog() == true && dlg.Event != null)
                    {
                        vm.AddCalendarEvent(dlg.Event);
                        vm.RefreshSelectedDateEvents();
                        vm.RefreshSelectedWeekEvents();
                        RenderPlanners();
                    }
                }
            }
        }

        private void WeekEvent_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border b && b.DataContext != null)
            {
                // DataContext will be anonymous; use reflection to get Time/Title
                var dc = b.DataContext;
                var time = dc.GetType().GetProperty("Time")?.GetValue(dc) as string;
                var title = dc.GetType().GetProperty("Title")?.GetValue(dc) as string;
                var vm = DataContext as ViewModels.MainViewModel;
                if (vm == null) return;

                // Find matching event by title/time on selected week
                var ev = vm.CalendarEvents.FirstOrDefault(x => x.Time == time && x.Title == title && x.Date.Date >= vm.SelectedDate.Date.AddDays(-3) && x.Date.Date <= vm.SelectedDate.Date.AddDays(3));
                if (ev != null)
                {
                    var copy = new CalendarEvent { Date = ev.Date, Time = ev.Time, Title = ev.Title, Description = ev.Description };
                    var dlg = new EventEditor(copy) { Owner = this };
                    if (dlg.ShowDialog() == true && dlg.Event != null)
                    {
                        ev.Date = dlg.Event.Date;
                        ev.Time = dlg.Event.Time;
                        ev.Title = dlg.Event.Title;
                        ev.Description = dlg.Event.Description;
                        vm.SaveCalendarEvents();
                        vm.RefreshSelectedDateEvents();
                        vm.RefreshSelectedWeekEvents();
                        RenderPlanners();
                    }
                }
            }
        }

        private void TierComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox combo && combo.SelectedItem is ComboBoxItem item && DataContext is ViewModels.MainViewModel vm)
            {
                var tierName = item.Content?.ToString();
                if (!string.IsNullOrEmpty(tierName))
                {
                    vm.ChangeTierCommand?.Execute(tierName);
                }
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Extract the selected theme name
            if (sender is not System.Windows.Controls.ComboBox combo) return;
            if (combo.SelectedItem is not ComboBoxItem item) return;
            if (DataContext is not ViewModels.MainViewModel vm) return;
            
            var themeName = item.Content?.ToString();
            if (string.IsNullOrEmpty(themeName) || vm.Theme == themeName) return;
            
            // Update the Theme property which will trigger instant theme application via PropertyChanged
            vm.Theme = themeName;
            
            // Auto-save the theme preference
            if (vm.SettingsService?.Settings != null)
            {
                vm.SettingsService.Settings.Theme = themeName;
                vm.SettingsService.Save();
            }
            }
        }
    }
}