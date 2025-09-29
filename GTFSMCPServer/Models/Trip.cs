using CsvHelper.Configuration.Attributes;

namespace GTFSMCPServer.Models;

/// <summary>
/// GTFS Trip - represents a journey taken by a vehicle through stops
/// </summary>
public class Trip
{
    [Name("route_id")]
    public string RouteId { get; set; } = string.Empty;

    [Name("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    [Name("trip_id")]
    public string TripId { get; set; } = string.Empty;

    [Name("trip_headsign")]
    public string? TripHeadsign { get; set; }

    [Name("trip_short_name")]
    public string? TripShortName { get; set; }

    [Name("direction_id")]
    public int? DirectionId { get; set; }

    [Name("block_id")]
    public string? BlockId { get; set; }

    [Name("shape_id")]
    public string? ShapeId { get; set; }
}
