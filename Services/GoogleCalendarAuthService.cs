using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Options;
using SwiftOpsToolbox.Models;
using GoogleCalendarService = Google.Apis.Calendar.v3.CalendarService;

namespace SwiftOpsToolbox.Services;

/// <summary>
/// Service for Google Calendar authentication and API access
/// </summary>
public class GoogleCalendarAuthService
{
    private readonly GoogleCalendarConfig _config;
    private UserCredential? _credential;
    private GoogleCalendarService? _calendarService;

    public GoogleCalendarAuthService(IOptions<GoogleCalendarConfig> config)
    {
        _config = config.Value;
    }

    /// <summary>
    /// Gets whether the user is currently authenticated with Google
    /// </summary>
    public bool IsAuthenticated => _credential != null && _calendarService != null;

    /// <summary>
    /// Authenticates with Google using OAuth2 flow
    /// </summary>
    /// <returns>True if authentication was successful</returns>
    public async Task<bool> AuthenticateAsync()
    {
        try
        {
            // Validate configuration
            if (string.IsNullOrWhiteSpace(_config.ClientId) || 
                string.IsNullOrWhiteSpace(_config.ClientSecret))
            {
                throw new InvalidOperationException(
                    "Google Calendar credentials are not configured. Please set ClientId and ClientSecret in appsettings.json");
            }

            // Create client secrets
            var clientSecrets = new ClientSecrets
            {
                ClientId = _config.ClientId,
                ClientSecret = _config.ClientSecret
            };

            // Authorize user and get credentials
            _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                _config.Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("SwiftOpsToolbox.GoogleCalendar"));

            // Create Calendar API service
            _calendarService = new GoogleCalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = _config.ApplicationName,
            });

            return true;
        }
        catch (Exception)
        {
            _credential = null;
            _calendarService = null;
            throw;
        }
    }

    /// <summary>
    /// Signs out the user and clears credentials
    /// </summary>
    public async Task SignOutAsync()
    {
        if (_credential != null)
        {
            await _credential.RevokeTokenAsync(CancellationToken.None);
            _credential = null;
        }
        _calendarService = null;
    }

    /// <summary>
    /// Gets the Calendar API service instance
    /// </summary>
    /// <returns>CalendarService instance if authenticated, null otherwise</returns>
    public GoogleCalendarService? GetCalendarService()
    {
        return _calendarService;
    }
}
