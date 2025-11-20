using SwiftOpsToolbox.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace SwiftOpsToolbox.Services
{
    /// <summary>
    /// Implementation of settings service that manages user tier and feature flags.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private UserSettings _settings;
        
        // Cache PropertyInfo objects for performance
        private static readonly Dictionary<string, PropertyInfo> _featureFlagProperties;

        public UserSettings Settings => _settings;

        public event EventHandler? SettingsChanged;

        static SettingsService()
        {
            // Initialize property cache once for all instances
            _featureFlagProperties = new Dictionary<string, PropertyInfo>();
            foreach (var prop in typeof(FeatureFlags).GetProperties())
            {
                if (prop.PropertyType == typeof(bool))
                {
                    _featureFlagProperties[prop.Name] = prop;
                }
            }
        }

        public SettingsService()
        {
            // Use AppData folder for settings
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "SwiftOpsToolbox");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            _settingsPath = Path.Combine(dir, "user-settings.json");

            // Initialize with default settings
            _settings = new UserSettings();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var loaded = JsonSerializer.Deserialize<UserSettings>(json);
                    if (loaded != null)
                    {
                        _settings = loaded;
                        
                        // Ensure feature flags are properly set based on tier
                        // This handles cases where the tier might have changed or features need updating
                        if (_settings.Features == null)
                        {
                            _settings.ApplyTierSettings();
                        }
                    }
                }
                else
                {
                    // First run - set up default Free tier
                    _settings = new UserSettings();
                    _settings.ApplyTierSettings();
                    Save();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to load settings: {ex.Message}");
                // On error, use default settings
                _settings = new UserSettings();
                _settings.ApplyTierSettings();
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_settingsPath, json);
                
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to save settings: {ex.Message}");
            }
        }

        public void SetTier(UserTier tier)
        {
            if (_settings.Tier != tier)
            {
                _settings.Tier = tier;
                _settings.ApplyTierSettings();
                Save();
            }
        }

        public bool IsFeatureEnabled(string featureName)
        {
            if (_settings?.Features == null)
            {
                return false;
            }

            // Use cached PropertyInfo for better performance
            if (_featureFlagProperties.TryGetValue(featureName, out var property))
            {
                return (bool)(property.GetValue(_settings.Features) ?? false);
            }

            return false;
        }
    }
}
