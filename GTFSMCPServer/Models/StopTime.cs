using CsvHelper.Configuration.Attributes;

namespace GTFSMCPServer.Models;

/// <summary>
/// GTFS StopTime - represents the times that a vehicle arrives at and departs from individual stops for each trip
/// </summary>
public class StopTime
{
    [Name("trip_id")]
    public string TripId { get; set; } = string.Empty;

    [Name("arrival_time")]
    public string ArrivalTime { get; set; } = string.Empty;

    [Name("departure_time")]
    public string DepartureTime { get; set; } = string.Empty;

    [Name("stop_id")]
    public string StopId { get; set; } = string.Empty;

    [Name("stop_sequence")]
    public int StopSequence { get; set; }

    [Name("stop_headsign")]
    public string? StopHeadsign { get; set; }

    [Name("pickup_type")]
    public int PickupType { get; set; } = 0;

    [Name("drop_off_type")]
    public int DropOffType { get; set; } = 0;

    [Name("shape_dist_traveled")]
    public double? ShapeDistanceTraveled { get; set; }

    // Helper property to get arrival time as TimeSpan
    public TimeSpan ArrivalTimeSpan => ParseGTFSTime(ArrivalTime);

    // Helper property to get departure time as TimeSpan
    public TimeSpan DepartureTimeSpan => ParseGTFSTime(DepartureTime);

    private static TimeSpan ParseGTFSTime(string time)
    {
        if (string.IsNullOrEmpty(time)) return TimeSpan.Zero;
        
        var parts = time.Split(':');
        if (parts.Length != 3) return TimeSpan.Zero;

        if (int.TryParse(parts[0], out int hours) &&
            int.TryParse(parts[1], out int minutes) &&
            int.TryParse(parts[2], out int seconds))
        {
            // GTFS allows hours > 24 for next day service
            return new TimeSpan(hours, minutes, seconds);
        }

        return TimeSpan.Zero;
    }
}
