using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using SwiftOpsToolbox.Models;

namespace SwiftOpsToolbox.Views
{
    public partial class MonthGridView : System.Windows.Controls.UserControl
    {
        public class DayVm
        {
            public int Day { get; set; }
            public ObservableCollection<CalendarEvent> Events { get; set; } = new ObservableCollection<CalendarEvent>();
            public DateTime Date { get; set; }
            public bool IsCurrentMonth { get; set; }
        }

        public MonthGridView()
        {
            InitializeComponent();
        }

        // Expose MonthTitle so XAML can bind and show current month
        public static readonly DependencyProperty MonthTitleProperty = DependencyProperty.Register(
            nameof(MonthTitle), typeof(string), typeof(MonthGridView), new PropertyMetadata(string.Empty));

        public string MonthTitle
        {
            get => (string)GetValue(MonthTitleProperty);
            set => SetValue(MonthTitleProperty, value);
        }

        public void SetMonth(DateTime date, IEnumerable<CalendarEvent> events)
        {
            // Set display title
            MonthTitle = date.ToString("MMMM yyyy");

            // Build 6x7 month grid starting Sunday
            var start = new DateTime(date.Year, date.Month, 1);
            // find previous Sunday
            int offset = ((int)start.DayOfWeek + 7) % 7;
            var gridStart = start.AddDays(-offset);

            var days = new List<DayVm>();
            for (int i = 0; i < 42; i++)
            {
                var d = gridStart.AddDays(i);
                var vm = new DayVm { Day = d.Day, Date = d, IsCurrentMonth = d.Month == date.Month };

                // include events that start/end across this date
                var dayEvents = events.Where(ev => ev.StartDate.Date <= d.Date && ev.EndDate.Date >= d.Date);
                foreach (var ev in dayEvents) vm.Events.Add(ev);
                days.Add(vm);
            }

            DaysItems.ItemsSource = days;
        }
    }
}