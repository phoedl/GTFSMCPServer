using CsvHelper.Configuration.Attributes;

namespace GTFSMCPServer.Models;

/// <summary>
/// GTFS Stop - represents a location where vehicles pick up or drop off riders
/// </summary>
public class Stop
{
    [Name("stop_id")]
    public string StopId { get; set; } = string.Empty;

    [Name("stop_name")]
    public string StopName { get; set; } = string.Empty;

    [Name("stop_lat")]
    public double StopLatitude { get; set; }

    [Name("stop_lon")]
    public double StopLongitude { get; set; }

    [Name("zone_id")]
    public string? ZoneId { get; set; }

    [Name("location_type")]
    public int? LocationType { get; set; }

    [Name("parent_station")]
    public string? ParentStation { get; set; }

    [Name("level_id")]
    public string? LevelId { get; set; }

    [Name("platform_code")]
    public string? PlatformCode { get; set; }
}
