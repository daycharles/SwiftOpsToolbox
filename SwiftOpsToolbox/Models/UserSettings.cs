namespace SwiftOpsToolbox.Models
{
    /// <summary>
    /// Represents user settings including their tier and customized feature preferences.
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// The user's subscription tier.
        /// </summary>
        public UserTier Tier { get; set; } = UserTier.Free;

        /// <summary>
        /// Feature flags that control which features are accessible.
        /// Automatically populated based on tier, but can be customized.
        /// </summary>
        public FeatureFlags Features { get; set; } = new FeatureFlags();

        // Existing settings
        public string Theme { get; set; } = "Dark";
        public bool StartOnCalendar { get; set; } = true;
        public bool Use24Hour { get; set; } = false;
        public string DefaultView { get; set; } = "Month";

        /// <summary>
        /// Updates feature flags based on the current tier.
        /// This ensures that features are properly enabled/disabled when tier changes.
        /// </summary>
        public void ApplyTierSettings()
        {
            Features = FeatureFlags.FromTier(Tier);
        }
    }
}
