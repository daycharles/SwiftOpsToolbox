using System;

namespace SwiftOpsToolbox.Models
{
    public class RemoteFile
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime Modified { get; set; }
        public bool IsDirectory { get; set; }
    }
}
