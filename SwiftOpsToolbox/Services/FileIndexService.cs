using SwiftOpsToolbox.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Security;
using System.Text.Json;

namespace SwiftOpsToolbox.Services
{
    // Very simple file indexer optimized for fast enumeration using background tasks.
    // Not as efficient as Everything (which uses NTFS MFT), but provides decent responsiveness
    // by incremental indexing and in-memory search.
    public class FileIndexService : IFileIndexService
    {
        private readonly ObservableCollection<SearchResult> _results = new ObservableCollection<SearchResult>();
        public ObservableCollection<SearchResult> Results => _results;

        // master index stored separately so searches can filter without losing the full index
        private readonly List<SearchResult> _indexList = new List<SearchResult>();

        private CancellationTokenSource? _cts;
        private readonly Dispatcher _dispatcher;
        private readonly object _locker = new object();

        public bool IsIndexing { get; private set; }
        public int IndexedCount { get { lock (_locker) { return _indexList.Count; } } }

        private string? _lastError;
        public string? LastError { get { lock (_locker) { return _lastError; } } }

        public event Action<int>? IndexProgressChanged;
        public event Action<bool>? IndexingStateChanged;
        public event Action<string>? IndexErrorOccurred;

        private readonly string _indexFilePath;
        private int _lastSavedCount = 0;
        private readonly int _saveBatchSize = 5000; // save every N new entries

        public FileIndexService()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            Logger.Log("FileIndexService initialized");

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "SwiftOpsToolbox");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            _indexFilePath = Path.Combine(dir, "index.json");

            // Load persisted index in background so UI can show results quickly
            Task.Run(async () => await LoadIndexAsync());
        }

        private async Task LoadIndexAsync()
        {
            try
            {
                if (!File.Exists(_indexFilePath)) return;

                Logger.Log("Loading persisted index from disk...");
                using var fs = File.OpenRead(_indexFilePath);
                var list = await JsonSerializer.DeserializeAsync<List<SearchResult>>(fs) ?? new List<SearchResult>();

                // populate index and UI in batches to avoid UI freeze
                var batchSize = 1000;
                for (int i = 0; i < list.Count; i += batchSize)
                {
                    var batch = list.Skip(i).Take(batchSize).ToList();
                    lock (_locker)
                    {
                        _indexList.AddRange(batch);
                    }

                    await _dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (var item in batch) _results.Add(item);
                    }));
                }

                _lastSavedCount = _indexList.Count;
                Logger.Log($"Loaded persisted index: {_indexList.Count} entries");
                IndexProgressChanged?.Invoke(IndexedCount);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private async Task SaveIndexAsync()
        {
            try
            {
                // write to temp then move to avoid corruption
                var temp = _indexFilePath + ".tmp";
                using (var fs = File.Create(temp))
                {
                    await JsonSerializer.SerializeAsync(fs, _indexList, new JsonSerializerOptions { WriteIndented = false });
                }
                File.Copy(temp, _indexFilePath, true);
                File.Delete(temp);
                _lastSavedCount = IndexedCount;
                Logger.Log($"Saved index to disk: {IndexedCount} entries");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void StartIndexing()
        {
            RefreshIndex();
        }

        public void StartIndexing(IEnumerable<string> roots)
        {
            RefreshIndex(roots);
        }

        public void StopIndexing()
        {
            _cts?.Cancel();
            // save index on stop
            Task.Run(async () => await SaveIndexAsync());
        }

        public void RefreshIndex()
        {
            // default to all drives
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady).Select(d => d.RootDirectory.FullName);
            RefreshIndex(drives);
        }

        public void RefreshIndex(IEnumerable<string> roots)
        {
            StopIndexing();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // clear previous index
            lock (_locker)
            {
                _indexList.Clear();
                _dispatcher.Invoke(() => _results.Clear());
            }

            IsIndexing = true;
            IndexingStateChanged?.Invoke(true);
            Logger.Log("Indexing started");

            Task.Run(async () =>
            {
                try
                {
                    foreach (var root in roots)
                    {
                        if (token.IsCancellationRequested) break;
                        try
                        {
                            if (Directory.Exists(root)) IndexDirectory(root, token);
                        }
                        catch (Exception ex)
                        {
                            // treat access/IO/path-too-long as non-fatal and skip
                            if (ex is UnauthorizedAccessException || ex is SecurityException || ex is PathTooLongException || ex is IOException)
                            {
                                Logger.Log($"Skipped root {root}: {ex.GetType().Name} {ex.Message}");
                                continue;
                            }

                            SetError(ex);
                        }

                        // periodically persist while indexing roots
                        if (IndexedCount - _lastSavedCount >= _saveBatchSize)
                        {
                            await SaveIndexAsync();
                        }
                    }
                }
                catch (Exception ex) { SetError(ex); }
                finally
                {
                    IsIndexing = false;
                    IndexingStateChanged?.Invoke(false);
                    Logger.Log("Indexing finished");
                    // final save
                    await SaveIndexAsync();
                }
            }, token);
        }

        private void SetError(Exception ex)
        {
            // For expected filesystem access errors, log and skip without surfacing as a UI error.
            if (ex is UnauthorizedAccessException || ex is SecurityException || ex is PathTooLongException || ex is IOException)
            {
                Logger.Log($"Indexer skipped path due to access/IO: {ex.GetType().Name} {ex.Message}");
                return;
            }

            lock (_locker)
            {
                _lastError = ex.Message;
            }
            IndexErrorOccurred?.Invoke(ex.Message);
            Logger.LogException(ex);
        }

        private void IndexDirectory(string path, CancellationToken token)
        {
            var toPush = new List<SearchResult>();
            try
            {
                var di = new DirectoryInfo(path);
                FileInfo[] files = Array.Empty<FileInfo>();
                try { files = di.GetFiles(); } catch (Exception ex) { SetError(ex); }

                foreach (var f in files)
                {
                    if (token.IsCancellationRequested) return;

                    var sr = new SearchResult { Path = f.FullName, SizeBytes = f.Length, Modified = f.LastWriteTime };

                    lock (_locker)
                    {
                        _indexList.Add(sr);
                        toPush.Add(sr);
                    }

                    // flush batch when it grows to avoid frequent dispatcher calls
                    if (toPush.Count >= 100)
                    {
                        var batch = new List<SearchResult>(toPush);
                        toPush.Clear();
                        // add batch on UI thread without blocking
                        _dispatcher.BeginInvoke(new Action(() =>
                        {
                            foreach (var item in batch) _results.Add(item);
                        }));
                    }

                    // notify progress periodically
                    if (IndexedCount % 100 == 0)
                    {
                        IndexProgressChanged?.Invoke(IndexedCount);
                        Logger.Log($"Indexed count: {IndexedCount}");
                    }
                }

                // push remaining from this directory
                if (toPush.Count > 0)
                {
                    var batch = new List<SearchResult>(toPush);
                    toPush.Clear();
                    _dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (var item in batch) _results.Add(item);
                    }));
                }

                DirectoryInfo[] subdirs = Array.Empty<DirectoryInfo>();
                try { subdirs = di.GetDirectories(); } catch (Exception ex) { SetError(ex); }
                foreach (var sd in subdirs)
                {
                    if (token.IsCancellationRequested) return;
                    try
                    {
                        // skip system folders and reparse points (junctions) which commonly cause access or recursion issues
                        var attrs = sd.Attributes;
                        if ((attrs & FileAttributes.System) == FileAttributes.System) continue;
                        if ((attrs & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) continue;
                        if (sd.Name.Equals("System Volume Information", System.StringComparison.OrdinalIgnoreCase)) continue;
                    }
                    catch { /* ignore attribute read errors and attempt to recurse - these are rare */ }

                    IndexDirectory(sd.FullName, token);
                }
            }
            catch (Exception ex) { SetError(ex); }
        }

        // simple search: filters master index by query tokens and updates the Results collection
        public void Search(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    // if empty query, show all indexed items
                    List<SearchResult> all;
                    lock (_locker)
                    {
                        all = _indexList.ToList();
                    }

                    // Use BeginInvoke and batch additions to avoid blocking the UI thread when large
                    _dispatcher.BeginInvoke(new Action(() =>
                    {
                        _results.Clear();
                        // add in small batches to keep UI responsive
                        const int batchSize = 500;
                        for (int i = 0; i < all.Count; i += batchSize)
                        {
                            var batch = all.Skip(i).Take(batchSize);
                            foreach (var r in batch) _results.Add(r);
                        }
                    }));

                    return;
                }

                var tokens = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).Where(t => t.Length > 0).ToArray();
                if (tokens.Length == 0)
                {
                    _dispatcher.BeginInvoke(new Action(() => _results.Clear()));
                    return;
                }

                List<SearchResult> matches;
                lock (_locker)
                {
                    // no artificial cap: return all matching entries
                    matches = _indexList.Where(r => tokens.All(t => r.Name.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
                }

                // update UI non-blocking and in batches
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    _results.Clear();
                    const int batchSize = 500;
                    for (int i = 0; i < matches.Count; i += batchSize)
                    {
                        var batch = matches.Skip(i).Take(batchSize);
                        foreach (var r in batch) _results.Add(r);
                    }
                }));
            }
            catch (Exception ex) { SetError(ex); }
        }
    }
}
