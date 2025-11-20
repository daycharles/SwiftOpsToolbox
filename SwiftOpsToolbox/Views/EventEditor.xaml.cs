using SwiftOpsToolbox.Models;
using System;
using System.Windows;

namespace SwiftOpsToolbox.Views
{
    public partial class EventEditor : Window
    {
        public CalendarEvent? Event { get; private set; }

        public EventEditor()
        {
            InitializeComponent();
        }

        public EventEditor(CalendarEvent ev) : this()
        {
            Event = ev;
            DpDate.SelectedDate = ev.Date;
            TbTime.Text = ev.Time ?? string.Empty;
            TbTitle.Text = ev.Title;
            TbDescription.Text = ev.Description;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (Event == null) Event = new CalendarEvent();
            var date = DpDate.SelectedDate ?? DateTime.Today;
            Event.Date = date.Date;
            Event.Time = TbTime.Text ?? string.Empty;
            Event.Title = TbTitle.Text ?? string.Empty;
            Event.Description = TbDescription.Text ?? string.Empty;
            DialogResult = true;
            Close();
        }
    }
}