# SwiftOpsToolbox

SwiftOps Toolbox is a fast, offline-friendly productivity suite. Includes calendar, to-do list, Markdown notes, lightning-fast file search, and secure SFTP client. Built for professionals who value speed, control, and simplicity—no cloud, no bloat.

## Features

### 📅 Calendar - Monthly Planner

- **Large Monthly View**: See an entire month at a glance with a clean, intuitive grid layout
- **Events Inside Day Cells**: View your events directly within each day, color-coded for easy identification
- **Event Management**:
  - Add new events with custom titles, descriptions, times, and colors
  - Edit existing events by clicking on them
  - Delete events when no longer needed
- **Day Details View**: Click any day to see a detailed list of all events scheduled for that date
- **Month Navigation**: Easily navigate between months using Previous/Next buttons
- **Visual Indicators**:
  - Today's date is highlighted with a blue circle
  - Events show time and title in color-coded badges
  - All-day events are clearly marked
  - Event overflow indicator shows when there are more than 3 events in a day
- **Google Calendar Integration** ⭐ NEW:
  - Sync your calendar with Google Calendar
  - View, create, edit, and delete events that sync across both platforms
  - Secure OAuth2 authentication
  - See [docs/GOOGLE_CALENDAR_SETUP.md](docs/GOOGLE_CALENDAR_SETUP.md) for setup instructions

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later

### Running the Application

```bash
dotnet run
```

Then navigate to `http://localhost:5000` in your web browser.

### Building the Application

```bash
dotnet build
```

### Google Calendar Integration (Optional)

To enable Google Calendar synchronization:

1. Follow the setup guide in [docs/GOOGLE_CALENDAR_SETUP.md](docs/GOOGLE_CALENDAR_SETUP.md)
2. Configure your OAuth credentials in `appsettings.json`
3. Navigate to Settings in the app and click "Connect Google Calendar"

The app works perfectly fine without Google Calendar integration - it's an optional feature for users who want cloud synchronization.

## Architecture

This is a Blazor Server application built with .NET 10.0. The codebase uses:

- Blazor Server with Interactive Server render mode
- Service-based architecture for extensibility
- Google Calendar API integration for cloud sync
- Modular design for easy feature additions

## Documentation

- [Google Calendar Setup Guide](docs/GOOGLE_CALENDAR_SETUP.md) - Complete setup instructions for Google Calendar integration
