using CsvHelper;
using GTFSMCPServer.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace GTFSMCPServer.Services;

/// <summary>
/// Service for loading and querying GTFS data
/// </summary>
public class GTFSDataService
{
    private readonly ILogger<GTFSDataService> _logger;
    private readonly Dictionary<string, Stop> _stops = new();
    private readonly Dictionary<string, Route> _routes = new();
    private readonly Dictionary<string, Trip> _trips = new();
    private readonly List<StopTime> _stopTimes = new();
    private readonly Dictionary<string, Models.Calendar> _calendars = new();
    private readonly Dictionary<string, Agency> _agencies = new();
    private bool _dataLoaded = false;

    public GTFSDataService(ILogger<GTFSDataService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Load GTFS data from specified directory
    /// </summary>
    public async Task LoadDataAsync(string gtfsDataPath)
    {
        try
        {
            _logger.LogInformation("Loading GTFS data from {Path}", gtfsDataPath);

            await LoadAgenciesAsync(Path.Combine(gtfsDataPath, "agency.txt"));
            await LoadStopsAsync(Path.Combine(gtfsDataPath, "stops.txt"));
            await LoadRoutesAsync(Path.Combine(gtfsDataPath, "routes.txt"));
            await LoadTripsAsync(Path.Combine(gtfsDataPath, "trips.txt"));
            await LoadStopTimesAsync(Path.Combine(gtfsDataPath, "stop_times.txt"));
            await LoadCalendarAsync(Path.Combine(gtfsDataPath, "calendar.txt"));

            _dataLoaded = true;
            _logger.LogInformation("GTFS data loaded successfully. Stops: {StopCount}, Routes: {RouteCount}, Trips: {TripCount}",
                _stops.Count, _routes.Count, _trips.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading GTFS data from {Path}", gtfsDataPath);
            throw;
        }
    }

    /// <summary>
    /// Find train connections between two stops
    /// </summary>
    public Task<List<TrainConnection>> FindConnectionsAsync(string fromStopId, string toStopId, DateTime date, TimeSpan? departureTime = null)
    {
        if (!_dataLoaded)
            throw new InvalidOperationException("GTFS data not loaded");

        var connections = new List<TrainConnection>();

        // Find all stop times for departure stop
        var departureStopTimes = _stopTimes
            .Where(st => st.StopId == fromStopId)
            .OrderBy(st => st.ArrivalTimeSpan)
            .ToList();

        foreach (var departureStopTime in departureStopTimes)
        {
            // Skip if departure time is specified and this trip departs too early
            if (departureTime.HasValue && departureStopTime.DepartureTimeSpan < departureTime.Value)
                continue;

            // Check if trip runs on the specified date
            if (!_trips.TryGetValue(departureStopTime.TripId, out var trip))
                continue;

            if (!IsServiceRunningOnDate(trip.ServiceId, date))
                continue;

            // Find arrival at destination stop for the same trip
            var arrivalStopTime = _stopTimes
                .FirstOrDefault(st => st.TripId == departureStopTime.TripId && 
                                     st.StopId == toStopId && 
                                     st.StopSequence > departureStopTime.StopSequence);

            if (arrivalStopTime != null && _routes.TryGetValue(trip.RouteId, out var route))
            {
                connections.Add(new TrainConnection
                {
                    TripId = trip.TripId,
                    RouteId = route.RouteId,
                    RouteShortName = route.RouteShortName ?? route.RouteLongName ?? route.RouteId,
                    RouteLongName = route.RouteLongName,
                    FromStopId = fromStopId,
                    ToStopId = toStopId,
                    FromStopName = _stops.TryGetValue(fromStopId, out var fromStop) ? fromStop.StopName : fromStopId,
                    ToStopName = _stops.TryGetValue(toStopId, out var toStop) ? toStop.StopName : toStopId,
                    DepartureTime = departureStopTime.DepartureTimeSpan,
                    ArrivalTime = arrivalStopTime.ArrivalTimeSpan,
                    Duration = arrivalStopTime.ArrivalTimeSpan - departureStopTime.DepartureTimeSpan,
                    TripHeadsign = trip.TripHeadsign
                });
            }
        }

        return Task.FromResult(connections.OrderBy(c => c.DepartureTime).ToList());
    }

    /// <summary>
    /// Find stops by name (partial match)
    /// </summary>
    public List<Stop> FindStopsByName(string searchTerm)
    {
        if (!_dataLoaded)
            throw new InvalidOperationException("GTFS data not loaded");

        return _stops.Values
            .Where(s => s.StopName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.StopName)
            .Take(20)
            .ToList();
    }

    /// <summary>
    /// Get stop by ID
    /// </summary>
    public Stop? GetStop(string stopId)
    {
        _stops.TryGetValue(stopId, out var stop);
        return stop;
    }

    /// <summary>
    /// Get all routes
    /// </summary>
    public List<Route> GetAllRoutes()
    {
        if (!_dataLoaded)
            throw new InvalidOperationException("GTFS data not loaded");

        return _routes.Values.OrderBy(r => r.RouteShortName ?? r.RouteLongName).ToList();
    }

    /// <summary>
    /// Get departures from a stop
    /// </summary>
    public List<Departure> GetDeparturesFromStop(string stopId, DateTime date, TimeSpan? fromTime = null)
    {
        if (!_dataLoaded)
            throw new InvalidOperationException("GTFS data not loaded");

        var departures = new List<Departure>();
        var stopTimes = _stopTimes
            .Where(st => st.StopId == stopId)
            .OrderBy(st => st.DepartureTimeSpan)
            .ToList();

        foreach (var stopTime in stopTimes)
        {
            if (fromTime.HasValue && stopTime.DepartureTimeSpan < fromTime.Value)
                continue;

            if (_trips.TryGetValue(stopTime.TripId, out var trip) && 
                IsServiceRunningOnDate(trip.ServiceId, date) &&
                _routes.TryGetValue(trip.RouteId, out var route))
            {
                departures.Add(new Departure
                {
                    TripId = trip.TripId,
                    RouteId = route.RouteId,
                    RouteShortName = route.RouteShortName ?? route.RouteLongName ?? route.RouteId,
                    DepartureTime = stopTime.DepartureTimeSpan,
                    Headsign = trip.TripHeadsign ?? stopTime.StopHeadsign,
                    StopId = stopId,
                    StopName = _stops.TryGetValue(stopId, out var stop) ? stop.StopName : stopId
                });
            }
        }

        return departures;
    }

    private bool IsServiceRunningOnDate(string serviceId, DateTime date)
    {
        if (!_calendars.TryGetValue(serviceId, out var calendar))
            return false;

        var startDate = calendar.GetStartDate();
        var endDate = calendar.GetEndDate();

        if (date < startDate || date > endDate)
            return false;

        return calendar.RunsOnDayOfWeek(date.DayOfWeek);
    }

    private async Task LoadAgenciesAsync(string filePath)
    {
        if (!File.Exists(filePath)) return;

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        // Configure CsvHelper to ignore missing headers for optional fields
        csv.Context.Configuration.HeaderValidated = null;
        csv.Context.Configuration.MissingFieldFound = null;
        
        var records = await Task.Run(() => csv.GetRecords<Agency>().ToList());
        foreach (var agency in records)
        {
            _agencies[agency.AgencyId ?? "default"] = agency;
        }
    }

    private async Task LoadStopsAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Required file not found: {filePath}");

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        // Configure CsvHelper to ignore missing headers for optional fields
        csv.Context.Configuration.HeaderValidated = null;
        csv.Context.Configuration.MissingFieldFound = null;
        
        var records = await Task.Run(() => csv.GetRecords<Stop>().ToList());
        foreach (var stop in records)
        {
            _stops[stop.StopId] = stop;
        }
    }

    private async Task LoadRoutesAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Required file not found: {filePath}");

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        // Configure CsvHelper to ignore missing headers for optional fields
        csv.Context.Configuration.HeaderValidated = null;
        csv.Context.Configuration.MissingFieldFound = null;
        
        var records = await Task.Run(() => csv.GetRecords<Route>().ToList());
        foreach (var route in records)
        {
            _routes[route.RouteId] = route;
        }
    }

    private async Task LoadTripsAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Required file not found: {filePath}");

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        // Configure CsvHelper to ignore missing headers for optional fields
        csv.Context.Configuration.HeaderValidated = null;
        csv.Context.Configuration.MissingFieldFound = null;
        
        var records = await Task.Run(() => csv.GetRecords<Trip>().ToList());
        foreach (var trip in records)
        {
            _trips[trip.TripId] = trip;
        }
    }

    private async Task LoadStopTimesAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Required file not found: {filePath}");

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        // Configure CsvHelper to ignore missing headers for optional fields
        csv.Context.Configuration.HeaderValidated = null;
        csv.Context.Configuration.MissingFieldFound = null;
        
        var records = await Task.Run(() => csv.GetRecords<StopTime>().ToList());
        _stopTimes.AddRange(records);
    }

    private async Task LoadCalendarAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Required file not found: {filePath}");

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        // Configure CsvHelper to ignore missing headers for optional fields
        csv.Context.Configuration.HeaderValidated = null;
        csv.Context.Configuration.MissingFieldFound = null;
        
        var records = await Task.Run(() => csv.GetRecords<Models.Calendar>().ToList());
        foreach (var calendar in records)
        {
            _calendars[calendar.ServiceId] = calendar;
        }
    }
}

/// <summary>
/// Represents a train connection between two stops
/// </summary>
public class TrainConnection
{
    public string TripId { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string RouteShortName { get; set; } = string.Empty;
    public string? RouteLongName { get; set; }
    public string FromStopId { get; set; } = string.Empty;
    public string ToStopId { get; set; } = string.Empty;
    public string FromStopName { get; set; } = string.Empty;
    public string ToStopName { get; set; } = string.Empty;
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string? TripHeadsign { get; set; }
}

/// <summary>
/// Represents a departure from a stop
/// </summary>
public class Departure
{
    public string TripId { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string RouteShortName { get; set; } = string.Empty;
    public TimeSpan DepartureTime { get; set; }
    public string? Headsign { get; set; }
    public string StopId { get; set; } = string.Empty;
    public string StopName { get; set; } = string.Empty;
}
