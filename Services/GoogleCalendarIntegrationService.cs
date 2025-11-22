using Google.Apis.Calendar.v3.Data;
using SwiftOpsToolbox.Models;
using GoogleCalendarService = Google.Apis.Calendar.v3.CalendarService;

namespace SwiftOpsToolbox.Services;

/// <summary>
/// Service for interacting with Google Calendar API
/// </summary>
public class GoogleCalendarIntegrationService
{
    private readonly GoogleCalendarAuthService _authService;

    public GoogleCalendarIntegrationService(GoogleCalendarAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Gets whether the service is connected to Google Calendar
    /// </summary>
    public bool IsConnected => _authService.IsAuthenticated;

    /// <summary>
    /// Retrieves events from Google Calendar for a specific month
    /// </summary>
    /// <param name="year">Year to retrieve events for</param>
    /// <param name="month">Month to retrieve events for</param>
    /// <returns>List of calendar events</returns>
    public async Task<List<CalendarEvent>> GetEventsForMonthAsync(int year, int month)
    {
        var service = _authService.GetCalendarService();
        if (service == null)
        {
            return new List<CalendarEvent>();
        }

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var request = service.Events.List("primary");
        request.TimeMinDateTimeOffset = new DateTimeOffset(startDate);
        request.TimeMaxDateTimeOffset = new DateTimeOffset(endDate);
        request.ShowDeleted = false;
        request.SingleEvents = true;
        request.OrderBy = Google.Apis.Calendar.v3.EventsResource.ListRequest.OrderByEnum.StartTime;

        var events = await request.ExecuteAsync();
        
        return events.Items
            .Select(MapGoogleEventToCalendarEvent)
            .Where(e => e != null)
            .Cast<CalendarEvent>()
            .ToList();
    }

    /// <summary>
    /// Creates a new event in Google Calendar
    /// </summary>
    /// <param name="calendarEvent">Event to create</param>
    /// <returns>The created event with Google Calendar ID</returns>
    public async Task<CalendarEvent?> CreateEventAsync(CalendarEvent calendarEvent)
    {
        var service = _authService.GetCalendarService();
        if (service == null)
        {
            return null;
        }

        var googleEvent = MapCalendarEventToGoogleEvent(calendarEvent);
        var createdEvent = await service.Events.Insert(googleEvent, "primary").ExecuteAsync();

        return MapGoogleEventToCalendarEvent(createdEvent);
    }

    /// <summary>
    /// Updates an existing event in Google Calendar
    /// </summary>
    /// <param name="calendarEvent">Event to update</param>
    /// <returns>The updated event</returns>
    public async Task<CalendarEvent?> UpdateEventAsync(CalendarEvent calendarEvent)
    {
        var service = _authService.GetCalendarService();
        if (service == null || string.IsNullOrEmpty(calendarEvent.GoogleEventId))
        {
            return null;
        }

        var googleEvent = MapCalendarEventToGoogleEvent(calendarEvent);
        var updatedEvent = await service.Events.Update(googleEvent, "primary", calendarEvent.GoogleEventId).ExecuteAsync();

        return MapGoogleEventToCalendarEvent(updatedEvent);
    }

    /// <summary>
    /// Deletes an event from Google Calendar
    /// </summary>
    /// <param name="googleEventId">Google Calendar event ID</param>
    public async Task DeleteEventAsync(string googleEventId)
    {
        var service = _authService.GetCalendarService();
        if (service == null || string.IsNullOrEmpty(googleEventId))
        {
            return;
        }

        await service.Events.Delete("primary", googleEventId).ExecuteAsync();
    }

    /// <summary>
    /// Maps a Google Calendar Event to a CalendarEvent model
    /// </summary>
    private CalendarEvent? MapGoogleEventToCalendarEvent(Event googleEvent)
    {
        if (googleEvent == null)
        {
            return null;
        }

        DateTime startTime, endTime;

        // Handle all-day events
        if (googleEvent.Start.DateTimeDateTimeOffset == null)
        {
            startTime = DateTime.Parse(googleEvent.Start.Date);
            endTime = DateTime.Parse(googleEvent.End.Date);
        }
        else
        {
            startTime = googleEvent.Start.DateTimeDateTimeOffset.Value.DateTime;
            endTime = googleEvent.End.DateTimeDateTimeOffset?.DateTime ?? startTime.AddHours(1);
        }

        return new CalendarEvent
        {
            GoogleEventId = googleEvent.Id,
            Title = googleEvent.Summary ?? "Untitled Event",
            Description = googleEvent.Description ?? string.Empty,
            StartTime = startTime,
            EndTime = endTime,
            Color = googleEvent.ColorId != null ? GetColorFromGoogleColorId(googleEvent.ColorId) : "#3788d8"
        };
    }

    /// <summary>
    /// Maps a CalendarEvent model to a Google Calendar Event
    /// </summary>
    private Event MapCalendarEventToGoogleEvent(CalendarEvent calendarEvent)
    {
        var googleEvent = new Event
        {
            Summary = calendarEvent.Title,
            Description = calendarEvent.Description,
        };

        // Handle all-day events
        if (calendarEvent.IsAllDay)
        {
            googleEvent.Start = new EventDateTime
            {
                Date = calendarEvent.StartTime.ToString("yyyy-MM-dd")
            };
            googleEvent.End = new EventDateTime
            {
                Date = calendarEvent.EndTime.ToString("yyyy-MM-dd")
            };
        }
        else
        {
            googleEvent.Start = new EventDateTime
            {
                DateTimeDateTimeOffset = new DateTimeOffset(calendarEvent.StartTime)
            };
            googleEvent.End = new EventDateTime
            {
                DateTimeDateTimeOffset = new DateTimeOffset(calendarEvent.EndTime)
            };
        }

        return googleEvent;
    }

    /// <summary>
    /// Gets a hex color from Google Calendar color ID
    /// </summary>
    private string GetColorFromGoogleColorId(string colorId)
    {
        // Google Calendar color palette
        return colorId switch
        {
            "1" => "#a4bdfc", // Lavender
            "2" => "#7ae7bf", // Sage
            "3" => "#dbadff", // Grape
            "4" => "#ff887c", // Flamingo
            "5" => "#fbd75b", // Banana
            "6" => "#ffb878", // Tangerine
            "7" => "#46d6db", // Peacock
            "8" => "#e1e1e1", // Graphite
            "9" => "#5484ed", // Blueberry
            "10" => "#51b749", // Basil
            "11" => "#dc2127", // Tomato
            _ => "#3788d8"
        };
    }
}
