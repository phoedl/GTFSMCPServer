using CsvHelper.Configuration.Attributes;

namespace GTFSMCPServer.Models;

/// <summary>
/// GTFS Calendar - defines service dates for service IDs using a weekly schedule
/// </summary>
public class Calendar
{
    [Name("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    [Name("monday")]
    public int Monday { get; set; }

    [Name("tuesday")]
    public int Tuesday { get; set; }

    [Name("wednesday")]
    public int Wednesday { get; set; }

    [Name("thursday")]
    public int Thursday { get; set; }

    [Name("friday")]
    public int Friday { get; set; }

    [Name("saturday")]
    public int Saturday { get; set; }

    [Name("sunday")]
    public int Sunday { get; set; }

    [Name("start_date")]
    public string StartDate { get; set; } = string.Empty;

    [Name("end_date")]
    public string EndDate { get; set; } = string.Empty;

    // Helper method to check if service runs on a specific day of week
    public bool RunsOnDayOfWeek(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => Monday == 1,
            DayOfWeek.Tuesday => Tuesday == 1,
            DayOfWeek.Wednesday => Wednesday == 1,
            DayOfWeek.Thursday => Thursday == 1,
            DayOfWeek.Friday => Friday == 1,
            DayOfWeek.Saturday => Saturday == 1,
            DayOfWeek.Sunday => Sunday == 1,
            _ => false
        };
    }

    // Helper method to parse GTFS date format (YYYYMMDD)
    public DateTime GetStartDate()
    {
        if (DateTime.TryParseExact(StartDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            return date;
        return DateTime.MinValue;
    }

    public DateTime GetEndDate()
    {
        if (DateTime.TryParseExact(EndDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            return date;
        return DateTime.MaxValue;
    }
}
