using System;

namespace SwiftOpsToolbox.Models
{
    public class SearchResult
    {
        public string Path { get; set; } = string.Empty;
        public string Name => System.IO.Path.GetFileName(Path);
        public long SizeBytes { get; set; }
        public DateTime Modified { get; set; }
    }
}