# GTFS MCP Server

A Model Context Protocol (MCP) server implementation in C# that provides functionality for querying train connection data in GTFS (General Transit Feed Specification) format.

## Features

- **Find train connections** between two stops on a specific date
- **Search for stops** by name
- **Get departures** from a specific stop
- **List all available routes**

## Setup

### Prerequisites

- .NET 8.0 SDK
- GTFS data files in CSV format

### Installation

1. Clone or download this project
2. Navigate to the project directory
3. Restore dependencies:
   ```bash
   dotnet restore
   ```

### GTFS Data Format

The server expects GTFS data in the standard format with these CSV files:

- `agency.txt` - Transit agencies
- `stops.txt` - Stop locations (required)
- `routes.txt` - Transit routes (required)
- `trips.txt` - Trips for each route (required)
- `stop_times.txt` - Times that vehicles arrive and depart from stops (required)
- `calendar.txt` - Service dates (required)

### Running the Server

#### Option 1: Command Line Argument
```bash
dotnet run "C:\path\to\gtfs\data"
```

#### Option 2: Environment Variable
```bash
set GTFS_DATA_PATH=C:\path\to\gtfs\data
dotnet run
```

#### Option 3: No GTFS Data (for testing MCP functionality)
```bash
dotnet run
```

## MCP Tools

The server exposes the following tools through the MCP protocol:

### 1. find_train_connections
Find train connections between two stops on a specific date.

**Parameters:**
- `from_stop_id` (required): ID of the departure stop
- `to_stop_id` (required): ID of the arrival stop  
- `date` (required): Date to search for connections (YYYY-MM-DD format)
- `departure_time` (optional): Earliest departure time (HH:MM format)

### 2. search_stops
Search for stops by name (partial matching supported).

**Parameters:**
- `search_term` (required): Search term to find stops

### 3. get_departures
Get departures from a specific stop on a given date.

**Parameters:**
- `stop_id` (required): ID of the stop
- `date` (required): Date to get departures for (YYYY-MM-DD format)
- `from_time` (optional): Earliest departure time (HH:MM format)

### 4. list_routes
List all available routes in the GTFS data.

**Parameters:** None

## Example Usage

Once the server is running, it communicates via JSON-RPC 2.0 over stdin/stdout according to the MCP protocol specification.

### Example Request
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "find_train_connections",
    "arguments": {
      "from_stop_id": "STOP_001",
      "to_stop_id": "STOP_002", 
      "date": "2024-01-15",
      "departure_time": "08:00"
    }
  }
}
```

## Configuration

Configuration can be set in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "GTFSDataPath": "/path/to/gtfs/data"
}
```

## Building for Production

```bash
dotnet publish -c Release -o ./publish
```

## License

This project is provided as-is for educational and development purposes.
