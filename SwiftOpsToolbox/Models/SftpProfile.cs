using CommunityToolkit.Mvvm.ComponentModel;

namespace SwiftOpsToolbox.Models
{
    public class SftpProfile : ObservableObject
    {
        private string _name = string.Empty;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _host = string.Empty;
        public string Host { get => _host; set => SetProperty(ref _host, value); }

        private int _port = 22;
        public int Port { get => _port; set => SetProperty(ref _port, value); }

        private string _username = string.Empty;
        public string Username { get => _username; set => SetProperty(ref _username, value); }

        private string _password = string.Empty; // optional: consider secure storage
        public string Password { get => _password; set => SetProperty(ref _password, value); }
    }
}
