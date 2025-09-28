using CsvHelper.Configuration.Attributes;

namespace GTFSMCPServer.Models;

/// <summary>
/// GTFS Stop - represents a location where vehicles pick up or drop off riders
/// </summary>
public class Stop
{
    [Name("stop_id")]
    public string StopId { get; set; } = string.Empty;

    [Name("stop_code")]
    public string? StopCode { get; set; }

    [Name("stop_name")]
    public string StopName { get; set; } = string.Empty;

    [Name("stop_desc")]
    public string? StopDescription { get; set; }

    [Name("stop_lat")]
    public double StopLatitude { get; set; }

    [Name("stop_lon")]
    public double StopLongitude { get; set; }

    [Name("zone_id")]
    public string? ZoneId { get; set; }

    [Name("stop_url")]
    public string? StopUrl { get; set; }

    [Name("location_type")]
    public int LocationType { get; set; } = 0;

    [Name("parent_station")]
    public string? ParentStation { get; set; }

    [Name("stop_timezone")]
    public string? StopTimezone { get; set; }

    [Name("wheelchair_boarding")]
    public int WheelchairBoarding { get; set; } = 0;
}
