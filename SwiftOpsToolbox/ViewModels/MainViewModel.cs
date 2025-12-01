using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SwiftOpsToolbox.Models;
using SwiftOpsToolbox.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows; // keep for types
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwiftOpsToolbox.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IClipboardService _clipboardService;
        public ObservableCollection<ClipboardItem> ClipboardItems => _clipboardService.Items;

        // File search service
        private readonly IFileIndexService _fileIndexService;
        public ObservableCollection<SearchResult> FileResults => _fileIndexService.Results;

        // SFTP service
        private readonly ISftpService _sftpService;
        public ISftpService SftpService => _sftpService;

        // Settings service for tier and feature management
        private readonly ISettingsService _settingsService;
        public ISettingsService SettingsService => _settingsService;

        public ICommand StartIndexingCommand { get; }
        public ICommand StartIndexRootsCommand { get; }
        public ICommand SearchFilesCommand { get; }
        public ICommand OpenFileExternalCommand { get; }
        public ICommand OpenFileInNotepadViewCommand { get; }

        public ICommand SftpConnectCommand { get; }
        public ICommand SftpDisconnectCommand { get; }
        public ICommand SftpListCommand { get; }

        // Event to request opening a file in notepad view
        public event Action<string>? OpenFileInNotepadRequested;

        // indexing state exposed to UI
        private bool _isIndexing;
        public bool IsIndexing { get => _isIndexing; private set => SetProperty(ref _isIndexing, value); }

        private int _indexedCount;
        public int IndexedCount { get => _indexedCount; private set => SetProperty(ref _indexedCount, value); }

        // To-do / calendar
        public ObservableCollection<TodoItem> ToDoItems { get; } = new ObservableCollection<TodoItem>();

        private string _newTodoText = string.Empty;
        public string NewTodoText
        {
            get => _newTodoText;
            set => SetProperty(ref _newTodoText, value);
        }

        public ICommand AddTodoCommand { get; }
        public ICommand RemoveTodoCommand { get; }

        // Clipboard commands
        public ICommand ClearClipboardCommand { get; }
        public ICommand RemoveClipboardItemCommand { get; }

        // Calendar
        public ObservableCollection<CalendarEvent> CalendarEvents { get; } = new ObservableCollection<CalendarEvent>();
        public ObservableCollection<CalendarEvent> UpcomingEvents { get; } = new ObservableCollection<CalendarEvent>();

        private DateTime _selectedDate = System.DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    // update month displays
                    OnPropertyChanged(nameof(SelectedDateMonth1));
                    OnPropertyChanged(nameof(SelectedDateMonth2));
                    OnPropertyChanged(nameof(SelectedDateMonth3));
                    RefreshSelectedDateEvents();
                    RefreshSelectedWeekEvents();
                }
            }
        }

        // Make these writable so bindings that try to set DisplayDate won't fail
        public DateTime SelectedDateMonth1
        {
            get => SelectedDate;
            set => SelectedDate = value;
        }

        public DateTime SelectedDateMonth2
        {
            get => SelectedDate.AddMonths(1);
            set => SelectedDate = value.AddMonths(-1);
        }

        public DateTime SelectedDateMonth3
        {
            get => SelectedDate.AddMonths(2);
            set => SelectedDate = value.AddMonths(-2);
        }

        private string _viewMode = "Month"; // Month, Week, Day
        public string ViewMode
        {
            get => _viewMode;
            set => SetProperty(ref _viewMode, value);
        }

        public ObservableCollection<CalendarEvent> SelectedDateEvents { get; } = new ObservableCollection<CalendarEvent>();
        public ObservableCollection<CalendarEvent> SelectedWeekEvents { get; } = new ObservableCollection<CalendarEvent>();

        public ICommand SetMonthViewCommand { get; }
        public ICommand SetWeekViewCommand { get; }
        public ICommand SetDayViewCommand { get; }

        // --- Dashboard / Home info properties ---
        private string _weatherLocation = "Local";
        public string WeatherLocation { get => _weatherLocation; set => SetProperty(ref _weatherLocation, value); }

        private string _weatherDescription = "Unknown";
        public string WeatherDescription { get => _weatherDescription; set => SetProperty(ref _weatherDescription, value); }

        private int _weatherTemperature = 0;
        public int WeatherTemperature { get => _weatherTemperature; set => SetProperty(ref _weatherTemperature, value); }

        private DateTime _weatherUpdated = System.DateTime.MinValue;
        public DateTime WeatherUpdated { get => _weatherUpdated; set => SetProperty(ref _weatherUpdated, value); }

        private string _machineName = string.Empty;
        public string MachineName { get => _machineName; set => SetProperty(ref _machineName, value); }

        private string _osVersion = string.Empty;
        public string OsVersion { get => _osVersion; set => SetProperty(ref _osVersion, value); }

        private string _uptime = string.Empty;
        public string Uptime { get => _uptime; set => SetProperty(ref _uptime, value); }

        // New properties used by XAML
        private string _currentTime = string.Empty;
        public string CurrentTime { get => _currentTime; set => SetProperty(ref _currentTime, value); }

        public string WeatherText => $"{WeatherLocation} · {WeatherDescription} · {WeatherTemperature}°C (Updated {(Use24Hour ? WeatherUpdated.ToString("HH:mm") : WeatherUpdated.ToString("hh:mm tt"))})";

        public string OSDescription => OsVersion;

        private long _appMemoryMb;
        public long AppMemoryMb { get => _appMemoryMb; set => SetProperty(ref _appMemoryMb, value); }

        public ICommand RefreshDashboardCommand { get; }
        public ICommand RefreshWeatherCommand => RefreshDashboardCommand;

        // --- Settings properties ---
        private string _theme = "Dark";
        public string Theme { get => _theme; set => SetProperty(ref _theme, value); }

        private bool _startOnCalendar = true;
        public bool StartOnCalendar { get => _startOnCalendar; set => SetProperty(ref _startOnCalendar, value); }

        private bool _use24Hour = false;
        public bool Use24Hour { get => _use24Hour; set { if (SetProperty(ref _use24Hour, value)) { OnPropertyChanged(nameof(WeatherText)); UpdateClockAndMemory(); } } }

        private string _defaultView = "Month";
        public string DefaultView { get => _defaultView; set => SetProperty(ref _defaultView, value); }

        // new: indexed roots settings
        private ObservableCollection<string> _indexedRoots = new ObservableCollection<string>();
        public ObservableCollection<string> IndexedRoots { get => _indexedRoots; set => SetProperty(ref _indexedRoots, value); }

        // Feature visibility properties based on user tier
        public bool CalendarVisible => _settingsService?.Settings?.Features?.CalendarEnabled ?? true;
        public bool TodoListVisible => _settingsService?.Settings?.Features?.TodoListEnabled ?? true;
        public bool NotepadVisible => _settingsService?.Settings?.Features?.NotepadEnabled ?? true;
        public bool FileSearchVisible => _settingsService?.Settings?.Features?.BasicFileSearchEnabled ?? true;
        public bool ClipboardVisible => _settingsService?.Settings?.Features?.ClipboardHistoryEnabled ?? true;
        public bool SftpVisible => _settingsService?.Settings?.Features?.SftpEnabled ?? false;
        
        // Advanced features (Pro tier)
        public bool AdvancedCalendarVisible => _settingsService?.Settings?.Features?.AdvancedCalendarEnabled ?? false;
        public bool AdvancedMarkdownVisible => _settingsService?.Settings?.Features?.AdvancedMarkdownEnabled ?? false;
        public bool AdvancedFileSearchVisible => _settingsService?.Settings?.Features?.AdvancedFileSearchEnabled ?? false;
        
        // Business features
        public bool TeamSharingVisible => _settingsService?.Settings?.Features?.TeamSharingEnabled ?? false;
        public bool CentralizedManagementVisible => _settingsService?.Settings?.Features?.CentralizedManagementEnabled ?? false;
        public bool AuditLogsVisible => _settingsService?.Settings?.Features?.AuditLogsEnabled ?? false;
        
        // Enterprise features
        public bool WhiteLabelingVisible => _settingsService?.Settings?.Features?.WhiteLabelingEnabled ?? false;
        public bool DirectoryIntegrationVisible => _settingsService?.Settings?.Features?.DirectoryIntegrationEnabled ?? false;
        public bool PrivateCloudVisible => _settingsService?.Settings?.Features?.PrivateCloudEnabled ?? false;
        
        // Current tier display and setter
        public string CurrentTierName
        {
            get => _settingsService?.Settings?.Tier.ToString() ?? "Free";
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                if (System.Enum.TryParse<UserTier>(value, true, out var tier) && tier != CurrentTier)
                {
                    CurrentTier = tier; // this updates settings service and refreshes UI
                }
            }
        }
        
        public UserTier CurrentTier 
        { 
            get => _settingsService?.Settings?.Tier ?? UserTier.Free;
            set
            {
                if (_settingsService?.Settings?.Tier != value)
                {
                    _settingsService.SetTier(value);
                    RefreshFeatureVisibility();
                }
            }
        }

        public ICommand SaveSettingsCommand { get; }
        public ICommand OpenLogCommand { get; }
        public ICommand ChangeTierCommand { get; }

        private readonly string _persistencePath;
        private readonly string _settingsPath;
        private readonly DispatcherTimer _timer;

        public MainViewModel()
        {
            _clipboardService = new ClipboardService();
            _clipboardService.Start();

            // Initialize settings service for tier and feature management
            _settingsService = new SettingsService();
            _settingsService.Load();
            _settingsService.SettingsChanged += OnSettingsServiceChanged;

            // file indexer
            _fileIndexService = new FileIndexService();
            StartIndexingCommand = new RelayCommand(() => _fileIndexService.StartIndexing());
            SearchFilesCommand = new RelayCommand<string>((q) => _fileIndexService.Search(q ?? string.Empty));
            OpenFileExternalCommand = new RelayCommand<SearchResult>((r) => { if (r != null) Process.Start(new ProcessStartInfo(r.Path) { UseShellExecute = true }); });
            OpenFileInNotepadViewCommand = new RelayCommand<SearchResult>((r) => { if (r != null) OpenFileInNotepadRequested?.Invoke(r.Path); });
            OpenLogCommand = new RelayCommand(() => Logger.OpenLog());
            StartIndexRootsCommand = new RelayCommand(() => _fileIndexService.StartIndexing(IndexedRoots));

            // SFTP service
            _sftpService = new SftpService();
            SftpConnectCommand = new RelayCommand(async () => { /* UI uses SftpService directly in the view handlers */ await Task.CompletedTask; });
            SftpDisconnectCommand = new RelayCommand(async () => await _sftpService.DisconnectAsync());
            SftpListCommand = new RelayCommand<string>(async (path) => { await _sftpService.ListDirectoryAsync(path ?? "/"); });

            // default indexed roots: prefer whole drives C:\ and D:\ when present, fallback to user profile
            var preferred = new[] { "C:\\", "D:\\" };
            var added = false;
            foreach (var p in preferred)
            {
                try
                {
                    if (Directory.Exists(p))
                    {
                        IndexedRoots.Add(p);
                        added = true;
                    }
                }
                catch { }
            }
            if (!added)
            {
                IndexedRoots.Add(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            }

            // subscribe to indexer progress
            _fileIndexService.IndexingStateChanged += (on) => {
                IsIndexing = on;
                // when indexing starts, update count
                IndexedCount = _fileIndexService.IndexedCount;
            };
            _fileIndexService.IndexProgressChanged += (count) => {
                IndexedCount = count;
            };
            _fileIndexService.IndexErrorOccurred += (msg) => {
                Logger.Log("Indexer error: " + msg);
            };

            AddTodoCommand = new RelayCommand(AddTodo, CanAddTodo);
            RemoveTodoCommand = new RelayCommand<TodoItem>(RemoveTodo);

            // clipboard commands
            ClearClipboardCommand = new RelayCommand(() => _clipboardService.Clear());
            RemoveClipboardItemCommand = new RelayCommand<ClipboardItem>(RemoveClipboardItem);

            SetMonthViewCommand = new RelayCommand(() => ViewMode = "Month");
            SetWeekViewCommand = new RelayCommand(() => ViewMode = "Week");
            SetDayViewCommand = new RelayCommand(() => ViewMode = "Day");

            RefreshDashboardCommand = new RelayCommand(RefreshDashboard);

            SaveSettingsCommand = new RelayCommand(SaveSettings);
            ChangeTierCommand = new RelayCommand<string>(ChangeTier);

            // persistence path in AppData
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "SwiftOpsToolbox");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            _persistencePath = Path.Combine(dir, "events.json");
            _settingsPath = Path.Combine(dir, "settings.json");

            // Load settings first
            LoadSettings();

            // Seed with a sample item
            ToDoItems.Add(new TodoItem { Text = "Welcome — add tasks here.", Timestamp = System.DateTime.Now });

            // Load persisted events
            LoadCalendarEvents();

            // If none loaded, seed samples
            if (CalendarEvents.Count == 0)
            {
                CalendarEvents.Add(new CalendarEvent { Date = System.DateTime.Today, Title = "Noon Meeting", Description = "Weekly sync", Time = "12:00" });
                CalendarEvents.Add(new CalendarEvent { Date = System.DateTime.Today.AddDays(2), Title = "Project Deadline", Description = "Submit report", Time = "09:00" });
            }

            // subscribe to collection changes to save and refresh derived lists
            CalendarEvents.CollectionChanged += (s, e) => { SaveCalendarEventsInternal(); RefreshUpcomingEvents(); RefreshSelectedDateEvents(); RefreshSelectedWeekEvents(); };

            RefreshSelectedDateEvents();
            RefreshSelectedWeekEvents();
            RefreshUpcomingEvents();

            // initialize dashboard data
            RefreshDashboard();

            // setup timer to update clock/memory every 30s
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _timer.Tick += (s, e) => UpdateClockAndMemory();
            _timer.Start();
            UpdateClockAndMemory();

            // START FILE INDEXING AUTOMATICALLY so FileSearchView has data
            try
            {
                _fileIndexService.StartIndexing();
            }
            catch { }
        }

        // event raised when settings are saved (so UI can react)
        public event System.EventHandler? SettingsSaved;

        private void OnSettingsServiceChanged(object? sender, EventArgs e)
        {
            // When settings service changes (e.g., tier updated), refresh UI bindings
            RefreshFeatureVisibility();
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }

        private void RefreshFeatureVisibility()
        {
            OnPropertyChanged(nameof(CalendarVisible));
            OnPropertyChanged(nameof(TodoListVisible));
            OnPropertyChanged(nameof(NotepadVisible));
            OnPropertyChanged(nameof(FileSearchVisible));
            OnPropertyChanged(nameof(ClipboardVisible));
            OnPropertyChanged(nameof(SftpVisible));
            OnPropertyChanged(nameof(AdvancedCalendarVisible));
            OnPropertyChanged(nameof(AdvancedMarkdownVisible));
            OnPropertyChanged(nameof(AdvancedFileSearchVisible));
            OnPropertyChanged(nameof(TeamSharingVisible));
            OnPropertyChanged(nameof(CentralizedManagementVisible));
            OnPropertyChanged(nameof(AuditLogsVisible));
            OnPropertyChanged(nameof(WhiteLabelingVisible));
            OnPropertyChanged(nameof(DirectoryIntegrationVisible));
            OnPropertyChanged(nameof(PrivateCloudVisible));
            OnPropertyChanged(nameof(CurrentTierName));
            OnPropertyChanged(nameof(CurrentTier));
        }

        private void ChangeTier(string? tierName)
        {
            if (string.IsNullOrWhiteSpace(tierName)) return;
            
            if (System.Enum.TryParse<UserTier>(tierName, true, out var tier))
            {
                _settingsService.SetTier(tier);
                RefreshFeatureVisibility();
            }
        }

        private void SaveSettings()
        {
            try
            {
                // Update the settings service with current UI preferences
                _settingsService.Settings.Theme = this.Theme;
                _settingsService.Settings.StartOnCalendar = this.StartOnCalendar;
                _settingsService.Settings.Use24Hour = this.Use24Hour;
                _settingsService.Settings.DefaultView = this.DefaultView;
                
                // Save through settings service (will trigger SettingsChanged event)
                _settingsService.Save();

                // Also save to legacy settings file for backward compatibility
                var obj = new { Theme = this.Theme, StartOnCalendar = this.StartOnCalendar, Use24Hour = this.Use24Hour, DefaultView = this.DefaultView, IndexedRoots = this.IndexedRoots };
                var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);

                // Notify property changes to update UI bindings
                OnPropertyChanged(nameof(Theme));
                OnPropertyChanged(nameof(StartOnCalendar));
                OnPropertyChanged(nameof(Use24Hour));
                OnPropertyChanged(nameof(DefaultView));
                OnPropertyChanged(nameof(IndexedRoots));

                // Apply some settings immediately where possible
                try
                {
                    // Update clock formatting immediately
                    UpdateClockAndMemory();
                    // Refresh dashboard values (weather/time/etc)
                    RefreshDashboard();

                    // Restart indexing using the configured roots
                    if (StartIndexRootsCommand != null && StartIndexRootsCommand.CanExecute(null))
                    {
                        StartIndexRootsCommand.Execute(null);
                    }
                }
                catch { }
            }
            catch { }
        }

        private void LoadSettings()
        {
            try
            {
                // Load from settings service first (has tier and feature flags)
                if (_settingsService?.Settings != null)
                {
                    Theme = _settingsService.Settings.Theme;
                    StartOnCalendar = _settingsService.Settings.StartOnCalendar;
                    Use24Hour = _settingsService.Settings.Use24Hour;
                    DefaultView = _settingsService.Settings.DefaultView;
                }

                // Also load from legacy settings file for backward compatibility
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var doc = JsonSerializer.Deserialize<JsonElement>(json);
                    if (doc.TryGetProperty("Theme", out var t)) Theme = t.GetString() ?? Theme;
                    if (doc.TryGetProperty("StartOnCalendar", out var sc)) StartOnCalendar = sc.GetBoolean();
                    if (doc.TryGetProperty("Use24Hour", out var u24)) Use24Hour = u24.GetBoolean();
                    if (doc.TryGetProperty("DefaultView", out var dv)) DefaultView = dv.GetString() ?? DefaultView;
                    if (doc.TryGetProperty("IndexedRoots", out var ir))
                    {
                        IndexedRoots.Clear();
                        foreach (var root in ir.EnumerateArray())
                        {
                            IndexedRoots.Add(root.GetString() ?? string.Empty);
                        }
                    }
                }
            }
            catch { }
        }

        private void RemoveClipboardItem(ClipboardItem? item)
        {
            if (item == null) return;
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_clipboardService.Items.Contains(item))
                        _clipboardService.Items.Remove(item);
                });
            }
            catch
            {
                // ignore
            }
        }

        private void UpdateClockAndMemory()
        {
            CurrentTime = Use24Hour ? DateTime.Now.ToString("g") : DateTime.Now.ToString("g");
            try
            {
                var proc = Process.GetCurrentProcess();
                AppMemoryMb = proc.WorkingSet64 / (1024 * 1024);
            }
            catch { }
        }

        private void LoadCalendarEvents()
        {
            try
            {
                if (File.Exists(_persistencePath))
                {
                    var json = File.ReadAllText(_persistencePath);
                    var list = JsonSerializer.Deserialize<List<CalendarEvent>>(json);
                    CalendarEvents.Clear();
                    if (list != null)
                    {
                        foreach (var ev in list) CalendarEvents.Add(ev);
                    }
                }
            }
            catch
            {
                // ignore load errors
            }
        }

        private void SaveCalendarEventsInternal()
        {
            try
            {
                var list = CalendarEvents.ToList();
                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_persistencePath, json);
            }
            catch
            {
                // ignore save errors
            }
        }

        public void SaveCalendarEvents() => SaveCalendarEventsInternal();

        public void RemoveCalendarEvent(CalendarEvent ev)
        {
            CalendarEvents.Remove(ev);
            SaveCalendarEventsInternal();
            RefreshSelectedDateEvents();
            RefreshSelectedWeekEvents();
            RefreshUpcomingEvents();
        }

        public void RefreshSelectedDateEvents()
        {
            SelectedDateEvents.Clear();
            foreach (var ev in CalendarEvents)
            {
                if (ev.Date.Date == SelectedDate.Date)
                    SelectedDateEvents.Add(ev);
            }
        }

        public void RefreshSelectedWeekEvents()
        {
            SelectedWeekEvents.Clear();
            // compute week start (Monday)
            var d = SelectedDate;
            int diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = d.AddDays(-diff).Date;
            for (int i = 0; i < 7; i++)
            {
                var day = weekStart.AddDays(i);
                var events = CalendarEvents.Where(ev => ev.Date.Date == day.Date);
                foreach (var ev in events)
                {
                    SelectedWeekEvents.Add(ev);
                }
            }
        }

        public void RefreshUpcomingEvents()
        {
            UpcomingEvents.Clear();
            var now = DateTime.Now.Date;
            var upcoming = CalendarEvents.Where(ev => ev.Date.Date >= now).OrderBy(ev => ev.Date).ThenBy(ev => ev.Time).Take(5);
            foreach (var ev in upcoming) UpcomingEvents.Add(ev);
        }

        private bool CanAddTodo() => !string.IsNullOrWhiteSpace(NewTodoText);

        private void AddTodo()
        {
            if (string.IsNullOrWhiteSpace(NewTodoText)) return;
            var item = new TodoItem { Text = NewTodoText.Trim(), Timestamp = System.DateTime.Now };
            ToDoItems.Insert(0, item);
            NewTodoText = string.Empty;
            // notify command can execute changed
            (AddTodoCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }

        private void RemoveTodo(TodoItem? item)
        {
            if (item is null) return;
            ToDoItems.Remove(item);
        }

        // Helper to add calendar events (could be bound to UI later)
        public void AddCalendarEvent(CalendarEvent ev)
        {
            CalendarEvents.Add(ev);
            if (ev.Date.Date == SelectedDate.Date) SelectedDateEvents.Add(ev);
            SaveCalendarEventsInternal();
            RefreshSelectedWeekEvents();
            RefreshUpcomingEvents();
        }

        // Simple dashboard refresher: populates system info and a sample weather snapshot.
        public void RefreshDashboard()
        {
            try
            {
                MachineName = Environment.MachineName;
                OsVersion = Environment.OSVersion.ToString();
                var uptimeTs = TimeSpan.FromMilliseconds(Environment.TickCount64);
                Uptime = $"{(int)uptimeTs.TotalHours}h {uptimeTs.Minutes}m";

                // Sample deterministic pseudo-weather based on day of year
                var seed = DateTime.Now.DayOfYear;
                var rnd = new System.Random(seed);
                var temps = new[] { 8, 10, 12, 14, 16, 18, 20, 22, 24, 26 };
                WeatherTemperature = temps[rnd.Next(0, temps.Length)];
                var descriptions = new[] { "Sunny", "Partly Cloudy", "Cloudy", "Rain", "Showers", "Windy", "Clear" };
                WeatherDescription = descriptions[seed % descriptions.Length];
                WeatherLocation = "Local";
                WeatherUpdated = DateTime.Now;

                // notify weather text changed
                OnPropertyChanged(nameof(WeatherText));
                OnPropertyChanged(nameof(OSDescription));
            }
            catch
            {
                // ignore
            }
        }
    }
}
