namespace SwiftOpsToolbox.Models;

/// <summary>
/// Configuration settings for Google Calendar OAuth authentication
/// </summary>
public class GoogleCalendarConfig
{
    /// <summary>
    /// OAuth 2.0 Client ID from Google Cloud Console
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth 2.0 Client Secret from Google Cloud Console
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Application name for Google API requests
    /// </summary>
    public string ApplicationName { get; set; } = "SwiftOpsToolbox";

    /// <summary>
    /// Scopes required for calendar access
    /// </summary>
    public string[] Scopes { get; set; } = new[] 
    { 
        "https://www.googleapis.com/auth/calendar",
        "https://www.googleapis.com/auth/calendar.events"
    };
}
