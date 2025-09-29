using CsvHelper.Configuration.Attributes;

namespace GTFSMCPServer.Models;

/// <summary>
/// GTFS Route - represents a group of trips that are displayed to riders as a single service
/// </summary>
public class Route
{
    [Name("route_id")]
    public string RouteId { get; set; } = string.Empty;

    [Name("agency_id")]
    public string? AgencyId { get; set; }

    [Name("route_short_name")]
    public string? RouteShortName { get; set; }

    [Name("route_long_name")]
    public string? RouteLongName { get; set; }

    [Name("route_type")]
    public int RouteType { get; set; }
}
