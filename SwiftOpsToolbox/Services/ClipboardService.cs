using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using SwiftOpsToolbox.Models;

namespace SwiftOpsToolbox.Services
{
	public class ClipboardService : IClipboardService
    {
		private readonly DispatcherTimer _timer;
		private string _lastText = string.Empty;
		public ObservableCollection<ClipboardItem> Items { get; } = new ObservableCollection<ClipboardItem>();

		public ClipboardService()
		{
			_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
			_timer.Tick += Timer_Tick;
		}

		public void Start() => _timer.Start();
		public void Stop() => _timer.Stop();
		public void Clear() => System.Windows.Application.Current.Dispatcher.Invoke(() => Items.Clear());

		private void Timer_Tick(object? sender, EventArgs e)
		{
			try
			{
				if (!System.Windows.Clipboard.ContainsText()) return;
				var text = System.Windows.Clipboard.GetText();
				if (string.IsNullOrEmpty(text)) return;
				if (text == _lastText) return;
				_lastText = text;
				System.Windows.Application.Current.Dispatcher.Invoke(() => Items.Insert(0, new ClipboardItem { Text = text, Timestamp = DateTime.Now }));
				if (Items.Count > 200) System.Windows.Application.Current.Dispatcher.Invoke(() => Items.RemoveAt(Items.Count - 1));
			}
			catch
			{
				// ignore clipboard exceptions
			}
		}
	}
}
