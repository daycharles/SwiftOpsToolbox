using SwiftOpsToolbox.Models;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;

namespace SwiftOpsToolbox.Services
{
    public interface IFileIndexService
    {
        ObservableCollection<SearchResult> Results { get; }
        void StartIndexing();
        void StartIndexing(IEnumerable<string> roots);
        void StopIndexing();
        void RefreshIndex();
        void RefreshIndex(IEnumerable<string> roots);
        void Search(string query);

        bool IsIndexing { get; }
        int IndexedCount { get; }

        event Action<int>? IndexProgressChanged; // passes new count
        event Action<bool>? IndexingStateChanged; // passes isIndexing
        event Action<string>? IndexErrorOccurred; // passes error message

        string? LastError { get; }
    }
}