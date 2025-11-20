using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SwiftOpsToolbox.Models;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Text.Json;
using System.Threading;

namespace SwiftOpsToolbox.Services
{
    public class SftpService : ISftpService, IDisposable
    {
        private SftpClient? _client;
        private readonly ObservableCollection<RemoteFile> _files = new ObservableCollection<RemoteFile>();
        public ObservableCollection<RemoteFile> Files => _files;

        private readonly ObservableCollection<SftpProfile> _profiles = new ObservableCollection<SftpProfile>();
        public ObservableCollection<SftpProfile> Profiles => _profiles;

        private readonly string _profilesPath;

        public event Action<string>? ConnectionStateChanged;

        // transfer events
        public event Action<string, ulong, ulong>? TransferProgressChanged;
        public event Action<string, bool, string?>? TransferCompleted;

        public SftpService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "SwiftOpsToolbox");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            _profilesPath = Path.Combine(dir, "sftp_profiles.json");

            LoadProfiles();
        }

        public async Task ConnectAsync(string host, int port, string username, string password)
        {
            await Task.Run(() =>
            {
                try
                {
                    DisconnectInternal();
                    _client = new SftpClient(host, port, username, password);
                    _client.Connect();
                    System.Windows.Application.Current?.Dispatcher.Invoke(() => ConnectionStateChanged?.Invoke("Connected"));
                }
                catch (Exception ex)
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(() => ConnectionStateChanged?.Invoke("Error: " + ex.Message));
                    throw;
                }
            });
        }

        public async Task ConnectAsync(SftpProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            await ConnectAsync(profile.Host, profile.Port, profile.Username, profile.Password);
        }

        public async Task DisconnectAsync()
        {
            await Task.Run(() => DisconnectInternal());
        }

        private void DisconnectInternal()
        {
            try
            {
                if (_client != null)
                {
                    if (_client.IsConnected) _client.Disconnect();
                    _client.Dispose();
                    _client = null;
                }
                System.Windows.Application.Current?.Dispatcher.Invoke(() => ConnectionStateChanged?.Invoke("Disconnected"));
            }
            catch { }
        }

        public async Task<IEnumerable<RemoteFile>> ListDirectoryAsync(string path)
        {
            if (_client == null || !_client.IsConnected) throw new InvalidOperationException("Not connected");

            return await Task.Run(() =>
            {
                var list = _client.ListDirectory(path)
                    .Where(e => e.Name != "." && e.Name != "..")
                    .Select(e => new RemoteFile
                    {
                        Name = e.Name,
                        Path = e.FullName,
                        SizeBytes = e.IsDirectory ? 0 : e.Length,
                        Modified = e.LastWriteTime,
                        IsDirectory = e.IsDirectory
                    }).ToList();

                // update observable collection on UI thread
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    _files.Clear();
                    foreach (var r in list) _files.Add(r);
                });

                return (IEnumerable<RemoteFile>)list;
            });
        }

        public async Task DownloadFileAsync(string remotePath, string localPath, CancellationToken cancellationToken = default)
        {
            if (_client == null || !_client.IsConnected) throw new InvalidOperationException("Not connected");

            await Task.Run(() =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // attempt to get remote file size for progress calculations
                    ulong total = 0;
                    try
                    {
                        var attrs = _client.GetAttributes(remotePath);
                        total = (ulong)attrs.Size;
                    }
                    catch { }

                    using var fs = File.Create(localPath);

                    // progress callback provided by SSH.NET
                    void progressCallback(ulong downloaded)
                    {
                        // raise progress on UI thread
                        System.Windows.Application.Current?.Dispatcher.Invoke(() => TransferProgressChanged?.Invoke(remotePath, downloaded, total));

                        if (cancellationToken.IsCancellationRequested)
                        {
                            // throw to abort transfer
                            throw new OperationCanceledException(cancellationToken);
                        }
                    }

                    _client.DownloadFile(remotePath, fs, progressCallback);

                    System.Windows.Application.Current?.Dispatcher.Invoke(() => TransferCompleted?.Invoke(remotePath, true, null));
                }
                catch (OperationCanceledException)
                {
                    // if cancelled, delete partial file if exists
                    try { if (File.Exists(localPath)) File.Delete(localPath); } catch { }
                    System.Windows.Application.Current?.Dispatcher.Invoke(() => TransferCompleted?.Invoke(remotePath, false, "Cancelled"));
                    throw;
                }
                catch (Exception ex)
                {
                    try { if (File.Exists(localPath)) File.Delete(localPath); } catch { }
                    System.Windows.Application.Current?.Dispatcher.Invoke(() => TransferCompleted?.Invoke(remotePath, false, ex.Message));
                    throw;
                }
            }, cancellationToken);
        }

        public async Task UploadFileAsync(string localPath, string remotePath, CancellationToken cancellationToken = default)
        {
            if (_client == null || !_client.IsConnected) throw new InvalidOperationException("Not connected");
            if (!File.Exists(localPath)) throw new FileNotFoundException("Local file not found", localPath);

            await Task.Run(() =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    ulong total = 0;
                    try
                    {
                        var fi = new FileInfo(localPath);
                        total = (ulong)fi.Length;
                    }
                    catch { }

                    using var fs = File.OpenRead(localPath);

                    void progressCallback(ulong uploaded)
                    {
                        System.Windows.Application.Current?.Dispatcher.Invoke(() => TransferProgressChanged?.Invoke(remotePath, uploaded, total));

                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException(cancellationToken);
                        }
                    }

                    _client.UploadFile(fs, remotePath, progressCallback);

                    System.Windows.Application.Current?.Dispatcher.Invoke(() => TransferCompleted?.Invoke(remotePath, true, null));
                }
                catch (OperationCanceledException)
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(() => TransferCompleted?.Invoke(remotePath, false, "Cancelled"));
                    throw;
                }
                catch (Exception ex)
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(() => TransferCompleted?.Invoke(remotePath, false, ex.Message));
                    throw;
                }
            }, cancellationToken);
        }

        public void AddProfile(SftpProfile p)
        {
            if (p == null) return;
            _profiles.Add(p);
        }

        public void RemoveProfile(SftpProfile p)
        {
            if (p == null) return;
            _profiles.Remove(p);
        }

        public void SaveProfiles()
        {
            try
            {
                var list = _profiles.ToList();
                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_profilesPath, json);
            }
            catch { }
        }

        public void LoadProfiles()
        {
            try
            {
                if (!File.Exists(_profilesPath)) return;
                var json = File.ReadAllText(_profilesPath);
                var list = JsonSerializer.Deserialize<List<SftpProfile>>(json);
                _profiles.Clear();
                if (list != null) foreach (var p in list) _profiles.Add(p);
            }
            catch { }
        }

        public void Dispose()
        {
            DisconnectInternal();
        }
    }
}
