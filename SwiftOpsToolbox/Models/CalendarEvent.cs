using System;

namespace SwiftOpsToolbox.Models
{
    public class CalendarEvent
    {
        // New model supports multi-day events via StartDate/EndDate
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Legacy compatibility: single-date apps used 'Date'
        public DateTime Date
        {
            get => StartDate;
            set => StartDate = value;
        }

        // Optional time (single time string for display)
        public string Time { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Color for event display (hex)
        public string Color { get; set; } = "#28a745";

        public bool IsMultiDay => EndDate.Date > StartDate.Date;

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Time))
                return $"{StartDate:d} {Time} - {Title}";
            return $"{StartDate:d} - {Title}";
        }
    }
}