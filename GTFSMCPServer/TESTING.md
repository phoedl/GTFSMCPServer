# Sample MCP Client Test

This document shows how to test the GTFS MCP Server using sample JSON-RPC requests.

## Test the Server

1. Build and run the server with sample data:
   ```bash
   dotnet run Data
   ```

2. The server will listen on stdin/stdout for JSON-RPC messages.

## Sample Requests

### 1. Initialize the Server
```json
{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "Test Client", "version": "1.0"}}}
```

### 2. List Available Tools
```json
{"jsonrpc": "2.0", "id": 2, "method": "tools/list", "params": {}}
```

### 3. Search for Stops
```json
{"jsonrpc": "2.0", "id": 3, "method": "tools/call", "params": {"name": "search_stops", "arguments": {"search_term": "Central"}}}
```

### 4. Find Train Connections
```json
{"jsonrpc": "2.0", "id": 4, "method": "tools/call", "params": {"name": "find_train_connections", "arguments": {"from_stop_id": "STOP_001", "to_stop_id": "STOP_002", "date": "2024-01-15"}}}
```

### 5. Get Departures
```json
{"jsonrpc": "2.0", "id": 5, "method": "tools/call", "params": {"name": "get_departures", "arguments": {"stop_id": "STOP_001", "date": "2024-01-15", "from_time": "07:00"}}}
```

### 6. List Routes
```json
{"jsonrpc": "2.0", "id": 6, "method": "tools/call", "params": {"name": "list_routes", "arguments": {}}}
```

## Sample Data

The project includes sample GTFS data in the `Data/` folder:
- 5 stops (Central Station, North Terminal, East Junction, West End, Airport Hub)
- 3 routes (Red Line, Blue Line, Green Line)
- Sample trips and schedules

## Testing with PowerShell

You can test the server using PowerShell:

```powershell
# Start the server
$process = Start-Process -FilePath "dotnet" -ArgumentList "run", "Data" -NoNewWindow -PassThru -RedirectStandardInput -RedirectStandardOutput

# Send a test request
$request = '{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "Test Client", "version": "1.0"}}}'
$process.StandardInput.WriteLine($request)
$response = $process.StandardOutput.ReadLine()
Write-Host $response
```
