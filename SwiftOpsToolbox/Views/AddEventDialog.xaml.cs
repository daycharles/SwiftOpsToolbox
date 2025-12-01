using SwiftOpsToolbox.Models;
using System;
using System.Windows;
using System.Windows.Media;

namespace SwiftOpsToolbox.Views
{
    public partial class AddEventDialog : Window
    {
        public CalendarEvent? Result { get; private set; }

        public AddEventDialog()
        {
            InitializeComponent();

            DpStart.SelectedDate = DateTime.Now.Date;
            DpEnd.SelectedDate = DateTime.Now.Date;

            BtnCancel.Click += (s, e) => { DialogResult = false; Close(); };
            BtnSave.Click += BtnSave_Click;
            BtnClose.Click += (s, e) => { DialogResult = false; Close(); };

            // wire color combo using FindName to avoid generated field issues
            if (FindName("CbColor") is System.Windows.Controls.ComboBox cb)
            {
                cb.SelectionChanged += CbColor_SelectionChanged;
                cb.SelectedIndex = 0;
            }

            // default times
            TxtStartTime.Text = "12:00";
            TxtEndTime.Text = "13:00";
        }

        private void CbColor_SelectionChanged(object? sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var cb = sender as System.Windows.Controls.ComboBox ?? FindName("CbColor") as System.Windows.Controls.ComboBox;
            var preview = FindName("ColorPreview") as System.Windows.Controls.Border;
            if (cb != null && preview != null)
            {
                if (cb.SelectedItem is System.Windows.Controls.ComboBoxItem it && it.Tag is string hex)
                {
                    try
                    {
                        var col = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
                        preview.Background = new SolidColorBrush(col);
                    }
                    catch { }
                }
            }
        }

        private void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitle.Text))
            {
                System.Windows.MessageBox.Show("Please enter a title.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var start = DpStart.SelectedDate ?? DateTime.Now.Date;
            var end = DpEnd.SelectedDate ?? start;
            if (end < start) end = start;

            var isAllDay = ChkAllDay.IsChecked ?? false;

            var timeText = TxtStartTime.Text ?? string.Empty;
            var endTimeText = TxtEndTime.Text ?? string.Empty;
            TimeSpan time = TimeSpan.Zero;
            TimeSpan endTime = TimeSpan.Zero;
            if (!string.IsNullOrWhiteSpace(timeText) && !isAllDay)
            {
                if (!TimeSpan.TryParse(timeText, out time))
                {
                    System.Windows.MessageBox.Show("Please enter start time as HH:mm.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            if (!string.IsNullOrWhiteSpace(endTimeText) && !isAllDay)
            {
                if (!TimeSpan.TryParse(endTimeText, out endTime))
                {
                    System.Windows.MessageBox.Show("Please enter end time as HH:mm.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var colorHex = "#28a745";
            var cb = FindName("CbColor") as System.Windows.Controls.ComboBox;
            if (cb != null && cb.SelectedItem is System.Windows.Controls.ComboBoxItem it && it.Tag is string hex) colorHex = hex;

            var ev = new CalendarEvent
            {
                StartDate = start.Date,
                EndDate = end.Date,
                Title = TxtTitle.Text.Trim(),
                Description = TxtDescription.Text.Trim(),
                Time = isAllDay ? string.Empty : (string.IsNullOrWhiteSpace(timeText) ? string.Empty : time.ToString(@"hh\:mm")),
                Color = colorHex
            };

            Result = ev;
            DialogResult = true;
            Close();
        }
    }
}