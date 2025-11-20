# Tiered Pricing & Feature Flags

This document describes the tiered pricing model and customizable feature system implemented in SwiftOps Toolbox.

## Overview

SwiftOps Toolbox now includes a flexible tiered pricing system that allows different feature sets to be enabled based on the user's subscription tier. This system is designed to:

- Enable easy customization of which features users can access
- Provide a foundation for future monetization strategies
- Allow users to experience different feature sets through a tier selector
- Maintain extensibility for future business and enterprise features

## Pricing Tiers

### 🆓 Free Tier (Starter)
The Free tier provides core productivity features:
- **Calendar & Events**: Basic recurring events and reminders
- **To-Do List**: Task management with timestamps
- **Notepad**: Markdown viewer with read/write capabilities and basic formatting
- **File Search**: Local file search with basic filters
- **Clipboard History**: Track and manage clipboard items

**Limitations:**
- No SFTP service
- No advanced search operators
- Community support only

### 💼 Pro Tier ($5–8/month)
All Free features plus:
- **Advanced Calendar**: Enhanced calendar features and views
- **Advanced Markdown**: Full markdown editing capabilities
- **Advanced File Search**: Search with advanced operators and filters
- **SFTP Client**: Secure file transfer protocol support
- **Priority Support**: Faster response times

### 🏢 Business Tier ($12–15/user/month)
All Pro features plus:
- **Team Sharing**: Collaborate with team members
- **Centralized Management**: Admin controls for team settings
- **Role-based SFTP**: Fine-grained access controls
- **Audit Logs**: Track user activities and changes
- **Email Support**: Dedicated email support channel

### 📊 Enterprise (Custom Pricing)
Everything in Business plus:
- **White-labeling**: Custom branding options
- **Directory Integration**: Connect with Active Directory/LDAP
- **Private Cloud**: Deploy on your own infrastructure
- **Dedicated Support**: Assigned support engineer

## Architecture

### Core Components

#### Models
- **UserTier.cs**: Enum defining the four subscription tiers
- **FeatureFlags.cs**: Boolean flags for each feature that can be enabled/disabled
- **UserSettings.cs**: Combined user settings including tier, feature flags, and preferences

#### Services
- **ISettingsService.cs**: Interface for settings management
- **SettingsService.cs**: Implementation that handles:
  - Loading/saving settings from persistent storage
  - Applying tier-based feature restrictions
  - Notifying UI of setting changes

#### ViewModels
- **MainViewModel.cs**: Integrates settings service and exposes feature visibility properties:
  - `CalendarVisible`, `TodoListVisible`, `NotepadVisible`
  - `FileSearchVisible`, `ClipboardVisible`, `SftpVisible`
  - `CurrentTier`, `CurrentTierName`

### UI Integration

The UI automatically responds to tier changes through:
- **Navigation Buttons**: Hidden when features are disabled
- **Settings Panel**: Shows current tier and available features
- **Tier Selector**: Demo ComboBox to test different tiers

## Usage

### For End Users

1. Open **Settings** from the left navigation panel
2. View your current tier under "Account Tier"
3. See which features are enabled in the "Available Features" section
4. (Demo mode only) Use the tier selector to simulate different subscription levels

### For Developers

#### Adding a New Feature Flag

1. Add a property to `FeatureFlags.cs`:
```csharp
public bool MyNewFeatureEnabled { get; set; } = false;
```

2. Update the `FromTier()` method to enable it for appropriate tiers:
```csharp
if (tier >= UserTier.Pro)
{
    flags.MyNewFeatureEnabled = true;
}
```

3. Add a visibility property to `MainViewModel.cs`:
```csharp
public bool MyNewFeatureVisible => _settingsService?.Settings?.Features?.MyNewFeatureEnabled ?? false;
```

4. Bind UI elements to the visibility property in XAML:
```xaml
<Button Visibility="{Binding MyNewFeatureVisible, Converter={StaticResource BooleanToVisibilityConverter}}" />
```

#### Checking Feature Availability in Code

```csharp
if (_settingsService.IsFeatureEnabled("SftpEnabled"))
{
    // Execute SFTP-related code
}
```

## Future Enhancements

This foundation enables several future improvements:

1. **License Validation**: Connect to a licensing server to validate tier status
2. **Subscription Management**: Integrate with payment processors (Stripe, PayPal)
3. **Trial Periods**: Implement time-limited trials of higher tiers
4. **Feature Usage Analytics**: Track which features are most valuable
5. **Dynamic Tier Restrictions**: Enforce limits server-side
6. **Team Management**: Admin portal for Business/Enterprise customers

## Technical Notes

### Performance
- Feature flag lookups use a cached property dictionary for optimal performance
- Settings are loaded once at startup and cached in memory
- UI updates use MVVM property notifications for efficient rendering

### Persistence
Settings are stored in JSON format at:
```
%APPDATA%\SwiftOpsToolbox\user-settings.json
```

Example settings file:
```json
{
  "Tier": 0,
  "Features": {
    "CalendarEnabled": true,
    "TodoListEnabled": true,
    "NotepadEnabled": true,
    "BasicFileSearchEnabled": true,
    "ClipboardHistoryEnabled": true,
    "AdvancedCalendarEnabled": false,
    "SftpEnabled": false
  },
  "Theme": "Dark",
  "StartOnCalendar": true,
  "Use24Hour": false,
  "DefaultView": "Month"
}
```

### Backward Compatibility
- Legacy settings files are still supported
- New installations default to Free tier
- Existing users maintain their current settings

## Testing

To test different tiers:

1. Launch the application
2. Navigate to Settings
3. Use the "Switch Tier (Demo)" dropdown
4. Observe how navigation buttons and features change
5. Try accessing features that are disabled in lower tiers

The tier selection is stored persistently, so it will be remembered across app restarts.
