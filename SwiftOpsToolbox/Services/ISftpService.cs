using SwiftOpsToolbox.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SwiftOpsToolbox.Services
{
    public interface ISftpService
    {
        ObservableCollection<RemoteFile> Files { get; }
        ObservableCollection<SftpProfile> Profiles { get; }
        event Action<string>? ConnectionStateChanged; // message

        // transfer events: remotePath, transferred bytes, total bytes
        event Action<string, ulong, ulong>? TransferProgressChanged;
        // transfer completed: remotePath, success, errorMessage
        event Action<string, bool, string?>? TransferCompleted;

        Task ConnectAsync(string host, int port, string username, string password);
        Task ConnectAsync(SftpProfile profile);
        Task DisconnectAsync();
        Task<IEnumerable<RemoteFile>> ListDirectoryAsync(string path);
        Task DownloadFileAsync(string remotePath, string localPath, CancellationToken cancellationToken = default);
        Task UploadFileAsync(string localPath, string remotePath, CancellationToken cancellationToken = default);

        void AddProfile(SftpProfile p);
        void RemoveProfile(SftpProfile p);
        void SaveProfiles();
        void LoadProfiles();
    }
}
