# GTFS MCP Server 🚂

A Model Context Protocol (MCP) server implementation in C# that provides functionality for querying Austrian train connection data in GTFS (General Transit Feed Specification) format. Built with real ÖBB (Austrian Federal Railways) schedule data.

## 🌟 Features

- **Find train connections by stop ID** - Direct connections between specific stops
- **Find connections by station name** - Smart search across all matching stations
- **Search for stops** by name with partial matching
- **Get departures** from specific stops with real-time schedules
- **List all available routes** - Browse complete Austrian railway network
- **Web and CLI support** - Dual-mode operation for flexibility
- **Azure deployment ready** - Container Apps compatible

## 🚀 Live Demo

The server is deployed and ready to use:
- **Production URL**: https://ca-gtfs-prod-uqobwqigygb2k.calmcliff-1fda861a.westeurope.azurecontainerapps.io/mcp
- **Test with Postman/curl**: Use the `/mcp` endpoint for HTTP requests
- **Austrian Railway Data**: Real ÖBB schedules with 7,141 stops, 246 routes, 9,615 trips

## 🛠️ Setup

### Prerequisites

- .NET 8.0 SDK
- Docker (for deployment)
- Azure CLI (optional, for deployment)
- GTFS data files in CSV format

### Local Installation

1. Clone this repository:
   ```bash
   git clone https://github.com/phoedl/GTFSMCPServer.git
   cd GTFSMCPServer
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Build the project:
   ```bash
   dotnet build
   ```

### 📊 GTFS Data

The server comes with real Austrian GTFS data from ÖBB (Austrian Federal Railways):

- **🚉 7,141 stops** - Complete Austrian railway network
- **🛤️ 246 routes** - Including CAT, S-Bahn, REX, regional services
- **🚄 9,615 trips** - Daily schedules with real timetables
- **📅 Service calendar** - Valid from 2024-12-15 to 2025-12-13

#### GTFS Data Format
Standard GTFS format with these CSV files:
- `agency.txt` - Transit agencies (ÖBB, etc.)
- `stops.txt` - Stop locations with Austrian station data
- `routes.txt` - Transit routes (CAT, S7, REX7, etc.)  
- `trips.txt` - Trips for each route with schedules
- `stop_times.txt` - Precise arrival/departure times
- `calendar.txt` - Service dates and validity periods

### 🏃‍♂️ Running the Server

#### Option 1: With Austrian GTFS Data (Recommended)
```bash
cd GTFSMCPServer
dotnet run -- Data
```

#### Option 2: Custom GTFS Data Path
```bash
dotnet run -- "C:\path\to\gtfs\data"
```

#### Option 3: Environment Variable
```bash
set GTFS_DATA_PATH=C:\path\to\gtfs\data
dotnet run
```

#### Option 4: Web Mode (HTTP Server)
```bash
dotnet run -- Data --web
```

The server supports two modes:
- **Standard Mode**: JSON-RPC over stdin/stdout (MCP protocol)
- **Web Mode**: HTTP server with `/mcp` endpoint for REST API calls

## 🔧 MCP Tools

The server exposes 5 powerful tools through the MCP protocol:

### 1. `find_train_connections`
Find direct train connections between two specific stops using stop IDs.

**Parameters:**
- `from_stop_id` (required): ID of the departure stop (e.g., "at:49:743:0:1")
- `to_stop_id` (required): ID of the arrival stop (e.g., "at:43:4708:0:2")
- `date` (required): Date to search for connections (YYYY-MM-DD format)
- `departure_time` (optional): Earliest departure time (HH:MM format)

### 2. `find_connections_by_name` ⭐ **NEW**
Find train connections using station names - searches all matching stations automatically!

**Parameters:**
- `from_station_name` (required): Name of departure station (e.g., "Wien Mitte")
- `to_station_name` (required): Name of arrival station (e.g., "Flughafen Wien")
- `date` (required): Date to search for connections (YYYY-MM-DD format)
- `departure_time` (optional): Earliest departure time (HH:MM format)
- `limit` (optional): Maximum connections to return (1-200, default: 50)

### 3. `search_stops`
Search for stops by name with intelligent partial matching.

**Parameters:**
- `search_term` (required): Search term to find stops (e.g., "Salzburg")

### 4. `get_departures`
Get departures from a specific stop with real-time schedules.

**Parameters:**
- `stop_id` (required): ID of the stop
- `date` (required): Date to get departures for (YYYY-MM-DD format)
- `from_time` (optional): Earliest departure time (HH:MM format)
- `limit` (optional): Maximum departures to return

### 5. `list_routes`
List all available routes in the Austrian railway network.

**Parameters:**
- `limit` (optional): Maximum routes to return

## 📝 Example Usage

### 🌐 HTTP API Examples (Web Mode)

#### Find connections between Wien Mitte and Vienna Airport:
```bash
curl -X POST "https://[deployment url]/mcp" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/call",
    "params": {
      "name": "find_connections_by_name",
      "arguments": {
        "from_station_name": "Wien Mitte",
        "to_station_name": "Flughafen Wien",
        "date": "2025-10-02",
        "departure_time": "08:00",
        "limit": 5
      }
    }
  }'
```

#### Search for stations:
```bash
curl -X POST "https://[deployment url]/mcp" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/call",
    "params": {
      "name": "search_stops",
      "arguments": {
        "search_term": "Salzburg"
      }
    }
  }'
```

### 🖥️ MCP Protocol Examples (CLI Mode)

#### Find direct connections by stop ID:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "find_train_connections",
    "arguments": {
      "from_stop_id": "at:49:743:0:1",
      "to_stop_id": "at:43:4708:0:2",
      "date": "2025-10-02",
      "departure_time": "08:00"
    }
  }
}
```

#### Get departures from Wien Mitte:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "get_departures",
    "arguments": {
      "stop_id": "at:49:743:0:1",
      "date": "2025-10-02",
      "from_time": "08:00",
      "limit": 10
    }
  }
}
```

## ⚙️ Configuration

Configuration can be set in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "GTFSMCPServer": "Information"
    }
  },
  "AllowedHosts": "*",
  "GTFSDataPath": "Data"
}
```

## 🐳 Docker Support

### Build Docker Image:
```bash
docker build -t gtfs-mcp-server .
```

### Run Container:
```bash
docker run -p 8080:8080 gtfs-mcp-server
```

## ☁️ Azure Deployment

### Prerequisites:
- Azure CLI installed and authenticated
- Azure Developer CLI (azd) installed

### Deploy to Azure Container Apps:
```bash
# Initialize (first time only)
azd init

# Deploy infrastructure and application
azd up
```

The deployment creates:
- Azure Container Registry
- Azure Container Apps Environment  
- Azure Application Insights
- Log Analytics Workspace
- Managed Identity for secure access

### Environment Variables:
- `ASPNETCORE_ENVIRONMENT`: Production
- `ASPNETCORE_URLS`: http://+:8080
- `GTFS_DATA_PATH`: Data

## 🧪 Testing

### Local Testing:
```bash
# Test with Austrian data
cd GTFSMCPServer
echo '{"jsonrpc": "2.0", "id": 1, "method": "tools/call", "params": {"name": "search_stops", "arguments": {"search_term": "Wien Mitte"}}}' | dotnet run -- Data
```

### PowerShell Testing Script:
```powershell
# Use the included test script
.\test-mcp-server.ps1
```

## 🗂️ Project Structure

```
GTFSMCPServer/
├── Data/                    # Austrian GTFS data files
│   ├── agency.txt
│   ├── calendar.txt
│   ├── routes.txt
│   ├── stops.txt
│   ├── stop_times.txt
│   └── trips.txt
├── Models/                  # GTFS data models
│   ├── Agency.cs
│   ├── Calendar.cs
│   ├── Route.cs
│   ├── Stop.cs
│   ├── StopTime.cs
│   └── Trip.cs
├── Protocol/                # MCP protocol implementation
│   ├── MCPMessages.cs
│   └── MCPTools.cs
├── Services/                # Core business logic
│   ├── GTFSDataService.cs
│   └── MCPServer.cs
├── Program.cs               # Application entry point
├── Dockerfile              # Container configuration
├── appsettings.json        # Application configuration
└── README.md               # This file
```

## 🚂 Real-World Data

This server includes real Austrian railway data featuring:

- **CAT (City Airport Train)**: Direct Wien Mitte ↔ Flughafen Wien (16 min)
- **S-Bahn**: Suburban railway network (S1, S2, S3, S7, etc.)
- **REX**: Regional express services
- **Railjet**: High-speed trains
- **Regional trains**: Local and regional connections

### Popular Routes:
- Wien Mitte → Flughafen Wien (CAT: 16 min, S7: 22 min)
- Salzburg → Innsbruck (A1/A3: 1h 48min)
- Wien → Graz (REX/IC services)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is provided as-is for educational and development purposes.

## 📞 Support

For issues and questions:
- Create an issue on GitHub
- Check the deployed service status at the Azure portal
- Test against the live endpoint for connectivity issues

---

**Built with ❤️ for the Austrian Railway Network using real ÖBB data**
