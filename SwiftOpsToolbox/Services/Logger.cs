using System;
using System.IO;
using System.Diagnostics;

namespace SwiftOpsToolbox.Services
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        public static string LogFilePath
        {
            get
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dir = Path.Combine(appData, "SwiftOpsToolbox");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return Path.Combine(dir, "indexer.log");
            }
        }

        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}" + Environment.NewLine;
                    File.AppendAllText(LogFilePath, line);
                }
            }
            catch
            {
                // ignore logging failures
            }
        }

        public static void LogException(Exception ex)
        {
            try
            {
                lock (_lock)
                {
                    var content = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Exception: {ex.Message}\n{ex.StackTrace}" + Environment.NewLine;
                    File.AppendAllText(LogFilePath, content);
                }
            }
            catch { }
        }

        public static string GetLogFilePath() => LogFilePath;

        public static void OpenLog()
        {
            try
            {
                var path = LogFilePath;
                if (!File.Exists(path)) File.WriteAllText(path, $"Log created at {DateTime.Now}\n");
                var psi = new ProcessStartInfo(path) { UseShellExecute = true };
                Process.Start(psi);
            }
            catch
            {
                // ignore
            }
        }
    }
}
