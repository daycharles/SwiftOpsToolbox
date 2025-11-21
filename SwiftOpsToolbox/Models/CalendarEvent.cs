using System;

namespace SwiftOpsToolbox.Models
{
    public class CalendarEvent
    {
        public DateTime Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Time))
                return $"{Date:d} {Time} - {Title}";
            return $"{Date:d} - {Title}";
        }
    }
}