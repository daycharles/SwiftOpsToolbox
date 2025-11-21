namespace SwiftOpsToolbox.Models
{
    /// <summary>
    /// Represents the subscription tier for a user.
    /// </summary>
    public enum UserTier
    {
        /// <summary>
        /// Free tier with basic features: Calendar, To-Do List, Notepad, and basic File Search.
        /// No SFTP service or advanced features.
        /// </summary>
        Free = 0,

        /// <summary>
        /// Pro tier ($5-8/month) with all Free features plus advanced calendar,
        /// markdown editing, file search with advanced operators, SFTP, and priority support.
        /// </summary>
        Pro = 1,

        /// <summary>
        /// Business tier ($12-15/user/month) with all Pro features plus team sharing,
        /// centralized management, role-based SFTP, audit logs, and email support.
        /// </summary>
        Business = 2,

        /// <summary>
        /// Enterprise tier (custom pricing) with everything in Business plus white-labeling,
        /// directory integration, private cloud deployment, and dedicated support.
        /// </summary>
        Enterprise = 3
    }
}
