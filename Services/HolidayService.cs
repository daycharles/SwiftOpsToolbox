using SwiftOpsToolbox.Models;

namespace SwiftOpsToolbox.Services;

public class HolidayService
{
    private readonly List<Holiday> _holidays = new();

    public HolidayService()
    {
        InitializeUSHolidays();
    }

    private void InitializeUSHolidays()
    {
        var currentYear = DateTime.Today.Year;
        
        // Initialize holidays for current year and next year
        for (int year = currentYear; year <= currentYear + 1; year++)
        {
            AddUSHolidays(year);
        }
    }

    private void AddUSHolidays(int year)
    {
        // Fixed date holidays
        _holidays.Add(new Holiday
        {
            Name = "New Year's Day",
            Date = new DateTime(year, 1, 1),
            Description = "First day of the year",
            Type = HolidayType.Federal,
            Color = "#dc3545"
        });

        _holidays.Add(new Holiday
        {
            Name = "Independence Day",
            Date = new DateTime(year, 7, 4),
            Description = "Celebrating US independence",
            Type = HolidayType.Federal,
            Color = "#0d6efd"
        });

        _holidays.Add(new Holiday
        {
            Name = "Veterans Day",
            Date = new DateTime(year, 11, 11),
            Description = "Honoring military veterans",
            Type = HolidayType.Federal,
            Color = "#198754"
        });

        _holidays.Add(new Holiday
        {
            Name = "Christmas Day",
            Date = new DateTime(year, 12, 25),
            Description = "Christian holiday celebrating the birth of Jesus Christ",
            Type = HolidayType.Federal,
            Color = "#dc3545"
        });

        _holidays.Add(new Holiday
        {
            Name = "Valentine's Day",
            Date = new DateTime(year, 2, 14),
            Description = "Day of love and romance",
            Type = HolidayType.Cultural,
            Color = "#e83e8c"
        });

        _holidays.Add(new Holiday
        {
            Name = "St. Patrick's Day",
            Date = new DateTime(year, 3, 17),
            Description = "Irish cultural celebration",
            Type = HolidayType.Cultural,
            Color = "#28a745"
        });

        _holidays.Add(new Holiday
        {
            Name = "Halloween",
            Date = new DateTime(year, 10, 31),
            Description = "Spooky celebration",
            Type = HolidayType.Cultural,
            Color = "#fd7e14"
        });

        // Moveable holidays (calculated based on specific rules)
        
        // Martin Luther King Jr. Day - Third Monday in January
        _holidays.Add(new Holiday
        {
            Name = "Martin Luther King Jr. Day",
            Date = GetNthWeekdayOfMonth(year, 1, DayOfWeek.Monday, 3),
            Description = "Honoring civil rights leader Martin Luther King Jr.",
            Type = HolidayType.Federal,
            Color = "#6f42c1"
        });

        // Presidents' Day - Third Monday in February
        _holidays.Add(new Holiday
        {
            Name = "Presidents' Day",
            Date = GetNthWeekdayOfMonth(year, 2, DayOfWeek.Monday, 3),
            Description = "Honoring US presidents",
            Type = HolidayType.Federal,
            Color = "#0d6efd"
        });

        // Memorial Day - Last Monday in May
        _holidays.Add(new Holiday
        {
            Name = "Memorial Day",
            Date = GetLastWeekdayOfMonth(year, 5, DayOfWeek.Monday),
            Description = "Honoring fallen military service members",
            Type = HolidayType.Federal,
            Color = "#dc3545"
        });

        // Labor Day - First Monday in September
        _holidays.Add(new Holiday
        {
            Name = "Labor Day",
            Date = GetNthWeekdayOfMonth(year, 9, DayOfWeek.Monday, 1),
            Description = "Celebrating American workers",
            Type = HolidayType.Federal,
            Color = "#ffc107"
        });

        // Columbus Day - Second Monday in October
        _holidays.Add(new Holiday
        {
            Name = "Columbus Day",
            Date = GetNthWeekdayOfMonth(year, 10, DayOfWeek.Monday, 2),
            Description = "Commemorating Christopher Columbus's arrival in the Americas",
            Type = HolidayType.Federal,
            Color = "#17a2b8"
        });

        // Thanksgiving - Fourth Thursday in November
        _holidays.Add(new Holiday
        {
            Name = "Thanksgiving Day",
            Date = GetNthWeekdayOfMonth(year, 11, DayOfWeek.Thursday, 4),
            Description = "Day of giving thanks",
            Type = HolidayType.Federal,
            Color = "#fd7e14"
        });

        // Juneteenth - June 19
        _holidays.Add(new Holiday
        {
            Name = "Juneteenth",
            Date = new DateTime(year, 6, 19),
            Description = "Commemorating the end of slavery in the United States",
            Type = HolidayType.Federal,
            Color = "#198754"
        });

        // Mother's Day - Second Sunday in May
        _holidays.Add(new Holiday
        {
            Name = "Mother's Day",
            Date = GetNthWeekdayOfMonth(year, 5, DayOfWeek.Sunday, 2),
            Description = "Honoring mothers and motherhood",
            Type = HolidayType.Cultural,
            Color = "#e83e8c"
        });

        // Father's Day - Third Sunday in June
        _holidays.Add(new Holiday
        {
            Name = "Father's Day",
            Date = GetNthWeekdayOfMonth(year, 6, DayOfWeek.Sunday, 3),
            Description = "Honoring fathers and fatherhood",
            Type = HolidayType.Cultural,
            Color = "#0d6efd"
        });
    }

    // Helper method to get the Nth occurrence of a weekday in a month
    private DateTime GetNthWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek, int occurrence)
    {
        var firstDayOfMonth = new DateTime(year, month, 1);
        var firstOccurrence = firstDayOfMonth;

        // Find the first occurrence of the desired day of week
        while (firstOccurrence.DayOfWeek != dayOfWeek)
        {
            firstOccurrence = firstOccurrence.AddDays(1);
        }

        // Add weeks to get to the Nth occurrence
        return firstOccurrence.AddDays(7 * (occurrence - 1));
    }

    // Helper method to get the last occurrence of a weekday in a month
    private DateTime GetLastWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek)
    {
        var lastDayOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        // Go backwards to find the last occurrence of the desired day of week
        while (lastDayOfMonth.DayOfWeek != dayOfWeek)
        {
            lastDayOfMonth = lastDayOfMonth.AddDays(-1);
        }

        return lastDayOfMonth;
    }

    public IEnumerable<Holiday> GetHolidaysForMonth(int year, int month)
    {
        // Ensure we have holidays for the requested year
        if (!_holidays.Any(h => h.Date.Year == year))
        {
            AddUSHolidays(year);
        }

        return _holidays
            .Where(h => h.Date.Year == year && h.Date.Month == month)
            .OrderBy(h => h.Date);
    }

    public IEnumerable<Holiday> GetHolidaysForDay(DateTime date)
    {
        return _holidays
            .Where(h => h.Date.Date == date.Date)
            .OrderBy(h => h.Name);
    }

    public IEnumerable<Holiday> GetAllHolidays()
    {
        return _holidays.OrderBy(h => h.Date);
    }
}
