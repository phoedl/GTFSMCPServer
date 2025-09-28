using CsvHelper.Configuration.Attributes;

namespace GTFSMCPServer.Models;

/// <summary>
/// GTFS Agency - represents a transit agency
/// </summary>
public class Agency
{
    [Name("agency_id")]
    public string? AgencyId { get; set; }

    [Name("agency_name")]
    public string AgencyName { get; set; } = string.Empty;

    [Name("agency_url")]
    public string AgencyUrl { get; set; } = string.Empty;

    [Name("agency_timezone")]
    public string AgencyTimezone { get; set; } = string.Empty;

    [Name("agency_lang")]
    public string? AgencyLanguage { get; set; }

    [Name("agency_phone")]
    public string? AgencyPhone { get; set; }

    [Name("agency_fare_url")]
    public string? AgencyFareUrl { get; set; }

    [Name("agency_email")]
    public string? AgencyEmail { get; set; }
}
