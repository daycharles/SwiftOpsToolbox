using SwiftOpsToolbox.Models;

namespace SwiftOpsToolbox.Services;

public class CalendarService
{
    private List<CalendarEvent> _events = new();
    private int _nextId = 1;

    public CalendarService()
    {
        // Add some sample events
        AddEvent(new CalendarEvent
        {
            Title = "Team Meeting",
            Description = "Weekly sync with the team",
            StartTime = DateTime.Today.AddDays(2).AddHours(10),
            EndTime = DateTime.Today.AddDays(2).AddHours(11),
            Color = "#3788d8"
        });

        AddEvent(new CalendarEvent
        {
            Title = "Project Deadline",
            Description = "Submit final report",
            StartTime = DateTime.Today.AddDays(5).AddHours(17),
            EndTime = DateTime.Today.AddDays(5).AddHours(18),
            Color = "#dc3545"
        });

        AddEvent(new CalendarEvent
        {
            Title = "Client Presentation",
            Description = "Q4 Results presentation",
            StartTime = DateTime.Today.AddDays(-3).AddHours(14),
            EndTime = DateTime.Today.AddDays(-3).AddHours(15),
            Color = "#28a745"
        });
        
        AddEvent(new CalendarEvent
        {
            Title = "Birthday Party",
            Description = "John's birthday celebration",
            StartTime = DateTime.Today.AddDays(10),
            EndTime = DateTime.Today.AddDays(10),
            Color = "#ffc107"
        });
    }

    public IEnumerable<CalendarEvent> GetEventsForMonth(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        return _events.Where(e => e.StartTime.Date >= startDate && e.StartTime.Date <= endDate)
                     .OrderBy(e => e.StartTime);
    }

    public IEnumerable<CalendarEvent> GetEventsForDay(DateTime date)
    {
        return _events.Where(e => e.StartTime.Date == date.Date)
                     .OrderBy(e => e.StartTime);
    }

    public void AddEvent(CalendarEvent calendarEvent)
    {
        calendarEvent.Id = _nextId++;
        _events.Add(calendarEvent);
    }

    public void UpdateEvent(CalendarEvent calendarEvent)
    {
        var existingEvent = _events.FirstOrDefault(e => e.Id == calendarEvent.Id);
        if (existingEvent != null)
        {
            existingEvent.Title = calendarEvent.Title;
            existingEvent.Description = calendarEvent.Description;
            existingEvent.StartTime = calendarEvent.StartTime;
            existingEvent.EndTime = calendarEvent.EndTime;
            existingEvent.Color = calendarEvent.Color;
        }
    }

    public void DeleteEvent(int id)
    {
        var eventToRemove = _events.FirstOrDefault(e => e.Id == id);
        if (eventToRemove != null)
        {
            _events.Remove(eventToRemove);
        }
    }
}
