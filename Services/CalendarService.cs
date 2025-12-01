using SwiftOpsToolbox.Models;

namespace SwiftOpsToolbox.Services;

public class CalendarService
{
    private List<CalendarEvent> _events = new();
    private int _nextId = 1;
    private readonly GoogleCalendarIntegrationService? _googleCalendarService;

    public CalendarService(GoogleCalendarIntegrationService? googleCalendarService = null)
    {
        _googleCalendarService = googleCalendarService;
        
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
    
    /// <summary>
    /// Gets whether Google Calendar is connected
    /// </summary>
    public bool IsGoogleCalendarConnected => _googleCalendarService?.IsConnected ?? false;

    public IEnumerable<CalendarEvent> GetEventsForMonth(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        return _events.Where(e => e.StartTime.Date >= startDate && e.StartTime.Date <= endDate)
                     .OrderBy(e => e.StartTime);
    }
    
    /// <summary>
    /// Synchronizes events from Google Calendar for the specified month
    /// </summary>
    public async Task SyncWithGoogleCalendarAsync(int year, int month)
    {
        if (_googleCalendarService?.IsConnected != true)
        {
            return;
        }

        try
        {
            var googleEvents = await _googleCalendarService.GetEventsForMonthAsync(year, month);
            
            // Merge Google Calendar events with local events
            // Remove existing Google events for this month and replace with fresh data
            _events.RemoveAll(e => !string.IsNullOrEmpty(e.GoogleEventId) && 
                                   e.StartTime.Year == year && 
                                   e.StartTime.Month == month);
            
            // Add Google events
            foreach (var googleEvent in googleEvents)
            {
                googleEvent.Id = _nextId++;
                _events.Add(googleEvent);
            }
        }
        catch
        {
            // Silently fail if sync fails - local calendar still works
        }
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
    
    /// <summary>
    /// Adds an event and optionally syncs to Google Calendar
    /// </summary>
    public async Task AddEventAsync(CalendarEvent calendarEvent)
    {
        calendarEvent.Id = _nextId++;
        _events.Add(calendarEvent);
        
        // Sync to Google Calendar if connected
        if (_googleCalendarService?.IsConnected == true)
        {
            try
            {
                var createdEvent = await _googleCalendarService.CreateEventAsync(calendarEvent);
                if (createdEvent != null)
                {
                    calendarEvent.GoogleEventId = createdEvent.GoogleEventId;
                }
            }
            catch
            {
                // Silently fail if sync fails - local event still created
            }
        }
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
    
    /// <summary>
    /// Updates an event and optionally syncs to Google Calendar
    /// </summary>
    public async Task UpdateEventAsync(CalendarEvent calendarEvent)
    {
        var existingEvent = _events.FirstOrDefault(e => e.Id == calendarEvent.Id);
        if (existingEvent != null)
        {
            existingEvent.Title = calendarEvent.Title;
            existingEvent.Description = calendarEvent.Description;
            existingEvent.StartTime = calendarEvent.StartTime;
            existingEvent.EndTime = calendarEvent.EndTime;
            existingEvent.Color = calendarEvent.Color;
            existingEvent.GoogleEventId = calendarEvent.GoogleEventId;
            
            // Sync to Google Calendar if connected
            if (_googleCalendarService?.IsConnected == true && !string.IsNullOrEmpty(existingEvent.GoogleEventId))
            {
                try
                {
                    await _googleCalendarService.UpdateEventAsync(existingEvent);
                }
                catch
                {
                    // Silently fail if sync fails - local event still updated
                }
            }
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
    
    /// <summary>
    /// Deletes an event and optionally removes it from Google Calendar
    /// </summary>
    public async Task DeleteEventAsync(int id)
    {
        var eventToRemove = _events.FirstOrDefault(e => e.Id == id);
        if (eventToRemove != null)
        {
            // Remove from Google Calendar if connected
            if (_googleCalendarService?.IsConnected == true && !string.IsNullOrEmpty(eventToRemove.GoogleEventId))
            {
                try
                {
                    await _googleCalendarService.DeleteEventAsync(eventToRemove.GoogleEventId);
                }
                catch
                {
                    // Silently fail if sync fails - local event still deleted
                }
            }
            
            _events.Remove(eventToRemove);
        }
    }
}
