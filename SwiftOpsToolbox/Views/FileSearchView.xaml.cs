using SwiftOpsToolbox.Models;
using SwiftOpsToolbox.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using System;
using System.Threading.Tasks;

namespace SwiftOpsToolbox.Views
{
    public partial class FileSearchView : System.Windows.Controls.UserControl
    {
        private readonly DispatcherTimer _debounceTimer;

        public FileSearchView()
        {
            InitializeComponent();

            _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _debounceTimer.Tick += DebounceTimer_Tick;

            Loaded += FileSearchView_Loaded;

            BtnOpenLog.Click += (s, e) =>
            {
                SwiftOpsToolbox.Services.Logger.OpenLog();
            };

            BtnSftpToggle.Click += (s, e) =>
            {
                SftpPanel.Visibility = SftpPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            };

            BtnSftpConnect.Click += async (s, e) =>
            {
                if (DataContext is MainViewModel vm)
                {
                    try
                    {
                        // Prefer connecting using selected profile if available
                        if (CbProfiles.SelectedItem is SftpProfile selectedProfile)
                        {
                            // update profile fields from UI in case user edited them before connecting
                            selectedProfile.Host = SftpHost.Text;
                            selectedProfile.Port = int.TryParse(SftpPort.Text, out var p2) ? p2 : 22;
                            selectedProfile.Username = SftpUser.Text;
                            selectedProfile.Password = SftpPass.Password;

                            await vm.SftpService.ConnectAsync(selectedProfile);
                        }
                        else
                        {
                            var host = SftpHost.Text;
                            var port = int.TryParse(SftpPort.Text, out var p) ? p : 22;
                            var user = SftpUser.Text;
                            var pass = SftpPass.Password;
                            await vm.SftpService.ConnectAsync(host, port, user, pass);
                        }

                        // refresh listing
                        await vm.SftpService.ListDirectoryAsync("/");
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("SFTP connect error: " + ex.Message);
                    }
                }
            };

            BtnSftpDisconnect.Click += async (s, e) =>
            {
                if (DataContext is MainViewModel vm)
                {
                    await vm.SftpService.DisconnectAsync();
                }
            };

            BtnProfileEdit.Click += (s, e) =>
            {
                if (DataContext is MainViewModel vm)
                {
                    // If a profile is selected, update it. Otherwise create a new one.
                    if (CbProfiles.SelectedItem is SftpProfile prof)
                    {
                        prof.Host = SftpHost.Text;
                        prof.Port = int.TryParse(SftpPort.Text, out var pp) ? pp : 22;
                        prof.Username = SftpUser.Text;
                        prof.Password = SftpPass.Password;
                        // Save profiles
                        vm.SftpService.SaveProfiles();
                        // refresh combo selection to show updated name if user edited host
                        CbProfiles.Items.Refresh();
                    }
                    else
                    {
                        var newProf = new SftpProfile
                        {
                            Name = SftpHost.Text,
                            Host = SftpHost.Text,
                            Port = int.TryParse(SftpPort.Text, out var pnew) ? pnew : 22,
                            Username = SftpUser.Text,
                            Password = SftpPass.Password
                        };
                        vm.SftpService.AddProfile(newProf);
                        vm.SftpService.SaveProfiles();
                        CbProfiles.SelectedItem = newProf;
                    }
                }
            };

            CbProfiles.SelectionChanged += (s, e) =>
            {
                if (DataContext is MainViewModel vm)
                {
                    if (CbProfiles.SelectedItem is SftpProfile prof)
                    {
                        SftpHost.Text = prof.Host;
                        SftpPort.Text = prof.Port.ToString();
                        SftpUser.Text = prof.Username;
                        SftpPass.Password = prof.Password;
                    }
                }
            };

            ResultsList.MouseDoubleClick += (s, e) =>
            {
                if (ResultsList.SelectedItem is SearchResult sr && DataContext is MainViewModel vm)
                {
                    if (vm.OpenFileExternalCommand.CanExecute(sr)) vm.OpenFileExternalCommand.Execute(sr);
                }
            };

            ResultsList.MouseRightButtonUp += (s, e) =>
            {
                // right-click currently opens notepad view if available
                if (ResultsList.SelectedItem is SearchResult sr && DataContext is MainViewModel vm)
                {
                    if (vm.OpenFileInNotepadViewCommand.CanExecute(sr)) vm.OpenFileInNotepadViewCommand.Execute(sr);
                }
            };

            RemoteList.MouseDoubleClick += async (s, e) =>
            {
                if (RemoteList.SelectedItem is RemoteFile rf && DataContext is MainViewModel vm)
                {
                    if (rf.IsDirectory)
                    {
                        await vm.SftpService.ListDirectoryAsync(rf.Path);
                    }
                    else
                    {
                        // download to temp and open externally
                        try
                        {
                            var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), rf.Name);
                            await vm.SftpService.DownloadFileAsync(rf.Path, tmp);
                            Process.Start(new ProcessStartInfo(tmp) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("Download error: " + ex.Message);
                        }
                    }
                }
            };
        }

        private void DebounceTimer_Tick(object? sender, EventArgs e)
        {
            _debounceTimer.Stop();
            if (DataContext is MainViewModel vm)
            {
                var q = SearchBox.Text ?? string.Empty;
                if (vm.SearchFilesCommand.CanExecute(q)) vm.SearchFilesCommand.Execute(q);
            }
        }

        private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            // restart debounce timer
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void FileSearchView_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // subscribe to the underlying service events (via reflection to access private field)
                var svc = typeof(MainViewModel).GetField("_fileIndexService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(vm) as Services.IFileIndexService;
                if (svc != null)
                {
                    svc.IndexingStateChanged += (isOn) => Dispatcher.Invoke(() => { IndexProgress.Visibility = isOn ? Visibility.Visible : Visibility.Collapsed; IndexStatus.Text = isOn ? "Indexing..." : "Idle"; });
                    svc.IndexProgressChanged += (count) => Dispatcher.Invoke(() => { CountText.Text = $"Indexed: {count}"; });
                    svc.IndexErrorOccurred += (msg) => Dispatcher.Invoke(() => { CountText.Text = $"Indexer error: {msg}"; IndexProgress.Visibility = Visibility.Collapsed; IndexStatus.Text = "Error"; });
                }

                // trigger an initial empty search to show a sample while indexing
                if (vm.SearchFilesCommand.CanExecute(string.Empty)) vm.SearchFilesCommand.Execute(string.Empty);
            }
        }
    }
}