namespace SwiftOpsToolbox.Models;

public class Holiday
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public HolidayType Type { get; set; }
    public string Color { get; set; } = "#dc3545"; // Red by default for holidays
}

public enum HolidayType
{
    Federal,
    Religious,
    Cultural,
    Other
}
