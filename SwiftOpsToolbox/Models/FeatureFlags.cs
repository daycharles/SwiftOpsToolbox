namespace SwiftOpsToolbox.Models
{
    /// <summary>
    /// Represents which features are enabled for a user based on their tier.
    /// This allows customization of the UI and feature access.
    /// </summary>
    public class FeatureFlags
    {
        // Core features (available in Free tier)
        public bool CalendarEnabled { get; set; } = true;
        public bool TodoListEnabled { get; set; } = true;
        public bool NotepadEnabled { get; set; } = true;
        public bool BasicFileSearchEnabled { get; set; } = true;
        public bool ClipboardHistoryEnabled { get; set; } = true;

        // Pro tier features
        public bool AdvancedCalendarEnabled { get; set; } = false;
        public bool AdvancedMarkdownEnabled { get; set; } = false;
        public bool AdvancedFileSearchEnabled { get; set; } = false;
        public bool SftpEnabled { get; set; } = false;

        // Business tier features
        public bool TeamSharingEnabled { get; set; } = false;
        public bool CentralizedManagementEnabled { get; set; } = false;
        public bool AuditLogsEnabled { get; set; } = false;

        // Enterprise tier features
        public bool WhiteLabelingEnabled { get; set; } = false;
        public bool DirectoryIntegrationEnabled { get; set; } = false;
        public bool PrivateCloudEnabled { get; set; } = false;

        /// <summary>
        /// Creates feature flags based on the user's tier.
        /// </summary>
        public static FeatureFlags FromTier(UserTier tier)
        {
            var flags = new FeatureFlags();

            // Free tier - basic features only (defaults)
            if (tier == UserTier.Free)
            {
                return flags;
            }

            // Pro tier - add advanced features
            if (tier >= UserTier.Pro)
            {
                flags.AdvancedCalendarEnabled = true;
                flags.AdvancedMarkdownEnabled = true;
                flags.AdvancedFileSearchEnabled = true;
                flags.SftpEnabled = true;
            }

            // Business tier - add collaboration features
            if (tier >= UserTier.Business)
            {
                flags.TeamSharingEnabled = true;
                flags.CentralizedManagementEnabled = true;
                flags.AuditLogsEnabled = true;
            }

            // Enterprise tier - add enterprise features
            if (tier >= UserTier.Enterprise)
            {
                flags.WhiteLabelingEnabled = true;
                flags.DirectoryIntegrationEnabled = true;
                flags.PrivateCloudEnabled = true;
            }

            return flags;
        }
    }
}
