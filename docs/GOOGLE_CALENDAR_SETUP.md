# Google Calendar Integration - Setup Guide

## Overview

SwiftOpsToolbox now supports integration with Google Calendar, allowing you to synchronize your local calendar events with your Google Calendar account. This guide walks you through setting up and using this feature.

## Features

- **OAuth2 Authentication**: Secure authentication flow using Google's OAuth2
- **Bidirectional Sync**: View Google Calendar events in SwiftOpsToolbox
- **Event Management**: Create, update, and delete events that sync to Google Calendar
- **Automatic Synchronization**: Events are automatically synced when navigating months
- **Privacy-Focused**: Your credentials are stored securely, and data is not permanently stored on our servers

## Prerequisites

1. A Google account
2. Access to [Google Cloud Console](https://console.cloud.google.com/)
3. .NET 10.0 SDK installed

## Setup Instructions

### Step 1: Create Google Cloud Project

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Click on the project dropdown and select "New Project"
3. Enter a project name (e.g., "SwiftOpsToolbox Calendar Integration")
4. Click "Create"

### Step 2: Enable Google Calendar API

1. In the Google Cloud Console, navigate to "APIs & Services" > "Library"
2. Search for "Google Calendar API"
3. Click on it and press "Enable"

### Step 3: Configure OAuth Consent Screen

1. Navigate to "APIs & Services" > "OAuth consent screen"
2. Select "External" user type (unless you have a Google Workspace)
3. Click "Create"
4. Fill in the required fields:
   - **App name**: SwiftOpsToolbox
   - **User support email**: Your email
   - **Developer contact information**: Your email
5. Click "Save and Continue"
6. On the Scopes page, click "Add or Remove Scopes"
7. Add the following scopes:
   - `https://www.googleapis.com/auth/calendar`
   - `https://www.googleapis.com/auth/calendar.events`
8. Click "Save and Continue"
9. Add test users (your Gmail address) if the app is in testing mode
10. Click "Save and Continue", then "Back to Dashboard"

### Step 4: Create OAuth 2.0 Credentials

1. Navigate to "APIs & Services" > "Credentials"
2. Click "Create Credentials" and select "OAuth client ID"
3. For Application type, select "Desktop app"
4. Enter a name (e.g., "SwiftOpsToolbox Desktop Client")
5. Click "Create"
6. A dialog will appear with your Client ID and Client Secret - **keep this window open**

### Step 5: Configure SwiftOpsToolbox

1. Open the `appsettings.json` file in your SwiftOpsToolbox installation directory
2. Locate the `GoogleCalendar` section:
   ```json
   "GoogleCalendar": {
     "ClientId": "",
     "ClientSecret": "",
     "ApplicationName": "SwiftOpsToolbox"
   }
   ```
3. Copy your **Client ID** from the Google Cloud Console and paste it into the `ClientId` field
4. Copy your **Client Secret** and paste it into the `ClientSecret` field
5. Save the file

Your `appsettings.json` should now look like:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "GoogleCalendar": {
    "ClientId": "123456789-abcdefg.apps.googleusercontent.com",
    "ClientSecret": "GOCSPX-abcdefghijklmnop",
    "ApplicationName": "SwiftOpsToolbox"
  }
}
```

### Step 6: Connect Your Google Account

1. **Restart** SwiftOpsToolbox if it was running
2. Navigate to the **Settings** page from the main menu
3. Click the **"Connect Google Calendar"** button
4. A browser window will open asking you to sign in to your Google account
5. Review the permissions requested and click **"Allow"**
6. You may see a warning that the app is not verified - click "Advanced" and then "Go to SwiftOpsToolbox (unsafe)" (this is safe because you created the credentials yourself)
7. The browser will show a success message, and you can close the window
8. Return to SwiftOpsToolbox - you should now see "Connected to Google Calendar"

### Step 7: Start Using the Integration

1. Navigate to the **Calendar** page
2. You should see a green "✅ Google Calendar Connected" status in the header
3. Click **"Sync with Google"** to fetch your Google Calendar events
4. Any events you create, edit, or delete in SwiftOpsToolbox will now sync to Google Calendar

## Usage

### Viewing Google Calendar Events

- Google Calendar events are automatically synced when you:
  - Open the Calendar page
  - Navigate to a different month
  - Click the "Sync with Google" button

### Creating Events

1. Click the **"Add Event"** button
2. Fill in the event details
3. Click **"Save"**
4. The event will be created both locally and in Google Calendar

### Editing Events

1. Click on any event in the calendar or day details view
2. Modify the event details
3. Click **"Save"**
4. Changes will be synced to Google Calendar

### Deleting Events

1. Click on an event to edit it
2. Click the **"Delete"** button
3. The event will be removed from both SwiftOpsToolbox and Google Calendar

### Disconnecting

1. Navigate to the **Settings** page
2. Click **"Disconnect"**
3. This will revoke SwiftOpsToolbox's access to your Google Calendar

## Troubleshooting

### Error: "Google Calendar credentials are not configured"

- Ensure you've added your Client ID and Client Secret to `appsettings.json`
- Verify the JSON syntax is correct (no missing commas or quotes)
- Restart the application after making changes

### OAuth Error: "redirect_uri_mismatch"

- Make sure you selected "Desktop app" as the application type when creating credentials
- If you selected "Web application", delete those credentials and create new ones as "Desktop app"

### Events Not Syncing

- Click the "Sync with Google" button manually
- Check your internet connection
- Try disconnecting and reconnecting your Google account
- Verify the Google Calendar API is enabled in your Google Cloud project

### "This app isn't verified" Warning

- This is normal for apps in testing mode with OAuth consent screen
- Click "Advanced" then "Go to SwiftOpsToolbox (unsafe)"
- This is safe because you created the credentials yourself
- To remove this warning, you would need to submit your app for Google's verification (not necessary for personal use)

## Security & Privacy

### Data Storage

- Your OAuth credentials are stored securely using Google's FileDataStore
- Credentials are stored in: `{UserProfile}/.credentials/SwiftOpsToolbox.GoogleCalendar/`
- Calendar events are synchronized but not permanently stored on our servers
- Events remain in your local database and Google Calendar

### Permissions

The integration requests the following Google Calendar scopes:
- `https://www.googleapis.com/auth/calendar` - View and manage calendars
- `https://www.googleapis.com/auth/calendar.events` - View and manage calendar events

### Revoking Access

To revoke SwiftOpsToolbox's access to your Google Calendar:

1. **From SwiftOpsToolbox**: Navigate to Settings > Click "Disconnect"
2. **From Google**: Visit https://myaccount.google.com/permissions and remove SwiftOpsToolbox

## Technical Details

### Dependencies

- `Google.Apis.Calendar.v3` (v1.72.0.3953)
- `Google.Apis.Auth` (v1.73.0)
- `Google.Apis` (v1.73.0)
- `Google.Apis.Core` (v1.73.0)

### Architecture

The Google Calendar integration consists of three main services:

1. **GoogleCalendarAuthService**: Handles OAuth2 authentication flow
2. **GoogleCalendarIntegrationService**: Manages API calls to Google Calendar
3. **CalendarService**: Extended to support sync with Google Calendar

### API Rate Limits

Google Calendar API has the following default quotas:
- Queries per day: 1,000,000
- Queries per 100 seconds per user: 250

For typical personal use, these limits are more than sufficient.

## Support

For issues or questions:
1. Check this documentation first
2. Review the [Google Calendar API documentation](https://developers.google.com/calendar)
3. Open an issue on the SwiftOpsToolbox GitHub repository

## Future Enhancements

Planned improvements:
- Support for multiple calendars
- Calendar color customization
- Event reminders and notifications
- Recurring event support improvements
- Offline mode with conflict resolution
