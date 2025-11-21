using SwiftOpsToolbox.Models;

namespace SwiftOpsToolbox.Services
{
    /// <summary>
    /// Service for managing user settings including tier and feature flags.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets the current user settings.
        /// </summary>
        UserSettings Settings { get; }

        /// <summary>
        /// Loads settings from persistent storage.
        /// </summary>
        void Load();

        /// <summary>
        /// Saves current settings to persistent storage.
        /// </summary>
        void Save();

        /// <summary>
        /// Updates the user's tier and applies corresponding feature flags.
        /// </summary>
        void SetTier(UserTier tier);

        /// <summary>
        /// Checks if a specific feature is enabled for the current user.
        /// </summary>
        bool IsFeatureEnabled(string featureName);

        /// <summary>
        /// Event raised when settings are saved or changed.
        /// </summary>
        event System.EventHandler? SettingsChanged;
    }
}
