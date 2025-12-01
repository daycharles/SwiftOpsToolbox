namespace SwiftOpsToolbox.Models;

public class CalendarEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Color { get; set; } = "#3788d8";
    
    /// <summary>
    /// Google Calendar event ID for synchronization
    /// </summary>
    public string? GoogleEventId { get; set; }
    
    public bool IsAllDay => StartTime.Date == EndTime.Date && 
                           StartTime.TimeOfDay == TimeSpan.Zero && 
                           EndTime.TimeOfDay == TimeSpan.Zero;
}
