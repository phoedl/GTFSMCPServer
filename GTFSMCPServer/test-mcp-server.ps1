# GTFS MCP Server Test Script
# This script demonstrates how to test the MCP server with sample requests

# Function to send a JSON-RPC request to the MCP server
function Send-MCPRequest {
    param(
        [string]$JsonRequest,
        [string]$ServerPath = "E:\Source\MCP\Test\GTFSMCPServer\bin\Debug\net8.0\GTFSMCPServer.exe",
        [string]$DataPath = "E:\Source\MCP\Test\GTFSMCPServer\Data"
    )
    
    Write-Host "Sending request: $JsonRequest" -ForegroundColor Yellow
    
    # Start the server process
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $ServerPath
    $psi.Arguments = $DataPath
    $psi.UseShellExecute = $false
    $psi.RedirectStandardInput = $true
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.CreateNoWindow = $true
    
    $process = [System.Diagnostics.Process]::Start($psi)
    
    # Send the request
    $process.StandardInput.WriteLine($JsonRequest)
    $process.StandardInput.Close()
    
    # Read the response
    $response = $process.StandardOutput.ReadToEnd()
    $errorOutput = $process.StandardError.ReadToEnd()
    
    $process.WaitForExit()
    
    if ($errorOutput) {
        Write-Host "Error: $errorOutput" -ForegroundColor Red
    }
    
    Write-Host "Response: $response" -ForegroundColor Green
    return $response
}

# Test 1: Initialize the server
Write-Host "`n=== Test 1: Initialize Server ===" -ForegroundColor Cyan
$initRequest = '{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "Test Client", "version": "1.0"}}}'
Send-MCPRequest -JsonRequest $initRequest

# Test 2: List available tools
Write-Host "`n=== Test 2: List Tools ===" -ForegroundColor Cyan
$toolsRequest = '{"jsonrpc": "2.0", "id": 2, "method": "tools/list", "params": {}}'
Send-MCPRequest -JsonRequest $toolsRequest

# Test 3: Search for stops
Write-Host "`n=== Test 3: Search Stops ===" -ForegroundColor Cyan
$searchRequest = '{"jsonrpc": "2.0", "id": 3, "method": "tools/call", "params": {"name": "search_stops", "arguments": {"search_term": "Central"}}}'
Send-MCPRequest -JsonRequest $searchRequest

# Test 4: Find train connections
Write-Host "`n=== Test 4: Find Connections ===" -ForegroundColor Cyan
$connectionsRequest = '{"jsonrpc": "2.0", "id": 4, "method": "tools/call", "params": {"name": "find_train_connections", "arguments": {"from_stop_id": "STOP_001", "to_stop_id": "STOP_002", "date": "2024-01-15"}}}'
Send-MCPRequest -JsonRequest $connectionsRequest

# Test 5: Get departures
Write-Host "`n=== Test 5: Get Departures ===" -ForegroundColor Cyan
$departuresRequest = '{"jsonrpc": "2.0", "id": 5, "method": "tools/call", "params": {"name": "get_departures", "arguments": {"stop_id": "STOP_001", "date": "2024-01-15", "from_time": "07:00"}}}'
Send-MCPRequest -JsonRequest $departuresRequest

# Test 6: List routes
Write-Host "`n=== Test 6: List Routes ===" -ForegroundColor Cyan
$routesRequest = '{"jsonrpc": "2.0", "id": 6, "method": "tools/call", "params": {"name": "list_routes", "arguments": {}}}'
Send-MCPRequest -JsonRequest $routesRequest

Write-Host "`n=== All Tests Completed ===" -ForegroundColor Green
