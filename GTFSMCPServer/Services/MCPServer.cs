using GTFSMCPServer.Protocol;
using GTFSMCPServer.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace GTFSMCPServer.Services;

/// <summary>
/// MCP Server implementation for GTFS data
/// </summary>
public class MCPServer
{
    private readonly GTFSDataService _gtfsService;
    private readonly ILogger<MCPServer> _logger;
    private readonly Dictionary<string, MCPTool> _tools;

    public MCPServer(GTFSDataService gtfsService, ILogger<MCPServer> logger)
    {
        _gtfsService = gtfsService;
        _logger = logger;
        _tools = CreateTools();
    }

    /// <summary>
    /// Start the MCP server
    /// </summary>
    public async Task StartAsync(string? gtfsDataPath = null)
    {
        _logger.LogInformation("Starting GTFS MCP Server...");

        // Load GTFS data if path is provided
        if (!string.IsNullOrEmpty(gtfsDataPath))
        {
            await _gtfsService.LoadDataAsync(gtfsDataPath);
        }

        _logger.LogInformation("MCP Server ready for requests");

        // Start processing messages from stdin/stdout
        await ProcessMessagesAsync();
    }

    /// <summary>
    /// Process MCP messages from stdin/stdout
    /// </summary>
    private async Task ProcessMessagesAsync()
    {
        var stdin = Console.OpenStandardInput();
        var stdout = Console.OpenStandardOutput();

        using var reader = new StreamReader(stdin, Encoding.UTF8);
        using var writer = new StreamWriter(stdout, Encoding.UTF8) { AutoFlush = true };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<MCPRequest>(line);
                if (request != null)
                {
                    var response = await HandleRequestAsync(request);
                    var responseJson = JsonConvert.SerializeObject(response, Formatting.None);
                    await writer.WriteLineAsync(responseJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", line);
                
                var errorResponse = new MCPResponse
                {
                    Id = "unknown",
                    Error = new MCPError
                    {
                        Code = -32700, // Parse error
                        Message = "Parse error"
                    }
                };
                
                var errorJson = JsonConvert.SerializeObject(errorResponse, Formatting.None);
                await writer.WriteLineAsync(errorJson);
            }
        }
    }

    /// <summary>
    /// Handle incoming MCP request
    /// </summary>
    private async Task<MCPResponse> HandleRequestAsync(MCPRequest request)
    {
        try
        {
            switch (request.Method)
            {
                case "initialize":
                    return await HandleInitializeAsync(request);
                
                case "tools/list":
                    return await HandleToolsListAsync(request);
                
                case "tools/call":
                    return await HandleToolCallAsync(request);
                
                default:
                    return new MCPResponse
                    {
                        Id = request.Id,
                        Error = new MCPError
                        {
                            Code = -32601, // Method not found
                            Message = $"Method not found: {request.Method}"
                        }
                    };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request: {Method}", request.Method);
            
            return new MCPResponse
            {
                Id = request.Id,
                Error = new MCPError
                {
                    Code = -32603, // Internal error
                    Message = "Internal error",
                    Data = ex.Message
                }
            };
        }
    }

    /// <summary>
    /// Handle initialize request
    /// </summary>
    private Task<MCPResponse> HandleInitializeAsync(MCPRequest request)
    {
        var result = new MCPInitializeResult
        {
            Capabilities = new MCPServerCapabilities
            {
                Tools = new MCPToolsCapability()
            },
            ServerInfo = new MCPServerInfo()
        };

        return Task.FromResult(new MCPResponse
        {
            Id = request.Id,
            Result = result
        });
    }

    /// <summary>
    /// Handle tools list request
    /// </summary>
    private Task<MCPResponse> HandleToolsListAsync(MCPRequest request)
    {
        var result = new
        {
            tools = _tools.Values.ToArray()
        };

        return Task.FromResult(new MCPResponse
        {
            Id = request.Id,
            Result = result
        });
    }

    /// <summary>
    /// Handle tool call request
    /// </summary>
    private async Task<MCPResponse> HandleToolCallAsync(MCPRequest request)
    {
        var toolCall = JsonConvert.DeserializeObject<MCPToolCall>(request.Params?.ToString() ?? "{}");
        if (toolCall == null)
        {
            return new MCPResponse
            {
                Id = request.Id,
                Error = new MCPError
                {
                    Code = -32602, // Invalid params
                    Message = "Invalid tool call parameters"
                }
            };
        }

        var result = await ExecuteToolAsync(toolCall);
        return new MCPResponse
        {
            Id = request.Id,
            Result = result
        };
    }

    /// <summary>
    /// Execute a tool call
    /// </summary>
    private async Task<MCPToolResult> ExecuteToolAsync(MCPToolCall toolCall)
    {
        try
        {
            switch (toolCall.Name)
            {
                case "find_train_connections":
                    return await FindTrainConnectionsAsync(toolCall.Arguments);
                
                case "search_stops":
                    return await SearchStopsAsync(toolCall.Arguments);
                
                case "get_departures":
                    return await GetDeparturesAsync(toolCall.Arguments);
                
                case "list_routes":
                    return await ListRoutesAsync(toolCall.Arguments);
                
                default:
                    return new MCPToolResult
                    {
                        Content = new List<MCPContent>
                        {
                            new MCPContent
                            {
                                Type = "text",
                                Text = $"Unknown tool: {toolCall.Name}"
                            }
                        },
                        IsError = true
                    };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool: {ToolName}", toolCall.Name);
            
            return new MCPToolResult
            {
                Content = new List<MCPContent>
                {
                    new MCPContent
                    {
                        Type = "text",
                        Text = $"Error executing tool {toolCall.Name}: {ex.Message}"
                    }
                },
                IsError = true
            };
        }
    }

    /// <summary>
    /// Find train connections between two stops
    /// </summary>
    private async Task<MCPToolResult> FindTrainConnectionsAsync(Dictionary<string, object>? arguments)
    {
        if (arguments == null || 
            !arguments.TryGetValue("from_stop_id", out var fromStopObj) ||
            !arguments.TryGetValue("to_stop_id", out var toStopObj) ||
            !arguments.TryGetValue("date", out var dateObj))
        {
            return new MCPToolResult
            {
                Content = new List<MCPContent>
                {
                    new MCPContent
                    {
                        Type = "text",
                        Text = "Missing required parameters: from_stop_id, to_stop_id, date"
                    }
                },
                IsError = true
            };
        }

        var fromStopId = fromStopObj.ToString() ?? "";
        var toStopId = toStopObj.ToString() ?? "";
        
        if (!DateTime.TryParse(dateObj.ToString(), out var date))
        {
            return new MCPToolResult
            {
                Content = new List<MCPContent>
                {
                    new MCPContent
                    {
                        Type = "text",
                        Text = "Invalid date format. Use YYYY-MM-DD"
                    }
                },
                IsError = true
            };
        }

        TimeSpan? departureTime = null;
        if (arguments.TryGetValue("departure_time", out var depTimeObj) &&
            TimeSpan.TryParse(depTimeObj.ToString(), out var depTime))
        {
            departureTime = depTime;
        }

        var connections = await _gtfsService.FindConnectionsAsync(fromStopId, toStopId, date, departureTime);
        
        var result = new StringBuilder();
        result.AppendLine($"Found {connections.Count} connections from {connections.FirstOrDefault()?.FromStopName ?? fromStopId} to {connections.FirstOrDefault()?.ToStopName ?? toStopId} on {date:yyyy-MM-dd}:");
        result.AppendLine();

        foreach (var connection in connections)
        {
            result.AppendLine($"Route {connection.RouteShortName}: {connection.DepartureTime:hh\\:mm} → {connection.ArrivalTime:hh\\:mm} (Duration: {connection.Duration:hh\\:mm})");
            if (!string.IsNullOrEmpty(connection.TripHeadsign))
            {
                result.AppendLine($"  Headsign: {connection.TripHeadsign}");
            }
            result.AppendLine();
        }

        if (connections.Count == 0)
        {
            result.AppendLine("No connections found for the specified criteria.");
        }

        return new MCPToolResult
        {
            Content = new List<MCPContent>
            {
                new MCPContent
                {
                    Type = "text",
                    Text = result.ToString()
                }
            }
        };
    }

    /// <summary>
    /// Search for stops by name
    /// </summary>
    private Task<MCPToolResult> SearchStopsAsync(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.TryGetValue("search_term", out var searchTermObj))
        {
            return Task.FromResult(new MCPToolResult
            {
                Content = new List<MCPContent>
                {
                    new MCPContent
                    {
                        Type = "text",
                        Text = "Missing required parameter: search_term"
                    }
                },
                IsError = true
            });
        }

        var searchTerm = searchTermObj.ToString() ?? "";
        var stops = _gtfsService.FindStopsByName(searchTerm);

        var result = new StringBuilder();
        result.AppendLine($"Found {stops.Count} stops matching '{searchTerm}':");
        result.AppendLine();

        foreach (var stop in stops)
        {
            result.AppendLine($"• {stop.StopName} (ID: {stop.StopId})");
            if (!string.IsNullOrEmpty(stop.StopDescription))
            {
                result.AppendLine($"  {stop.StopDescription}");
            }
        }

        if (stops.Count == 0)
        {
            result.AppendLine("No stops found matching the search term.");
        }

        return Task.FromResult(new MCPToolResult
        {
            Content = new List<MCPContent>
            {
                new MCPContent
                {
                    Type = "text",
                    Text = result.ToString()
                }
            }
        });
    }

    /// <summary>
    /// Get departures from a stop
    /// </summary>
    private Task<MCPToolResult> GetDeparturesAsync(Dictionary<string, object>? arguments)
    {
        if (arguments == null || 
            !arguments.TryGetValue("stop_id", out var stopIdObj) ||
            !arguments.TryGetValue("date", out var dateObj))
        {
            return Task.FromResult(new MCPToolResult
            {
                Content = new List<MCPContent>
                {
                    new MCPContent
                    {
                        Type = "text",
                        Text = "Missing required parameters: stop_id, date"
                    }
                },
                IsError = true
            });
        }

        var stopId = stopIdObj.ToString() ?? "";
        
        if (!DateTime.TryParse(dateObj.ToString(), out var date))
        {
            return Task.FromResult(new MCPToolResult
            {
                Content = new List<MCPContent>
                {
                    new MCPContent
                    {
                        Type = "text",
                        Text = "Invalid date format. Use YYYY-MM-DD"
                    }
                },
                IsError = true
            });
        }

        TimeSpan? fromTime = null;
        if (arguments.TryGetValue("from_time", out var fromTimeObj) &&
            TimeSpan.TryParse(fromTimeObj.ToString(), out var fTime))
        {
            fromTime = fTime;
        }

        var departures = _gtfsService.GetDeparturesFromStop(stopId, date, fromTime);
        
        var result = new StringBuilder();
        var stop = _gtfsService.GetStop(stopId);
        result.AppendLine($"Departures from {stop?.StopName ?? stopId} on {date:yyyy-MM-dd}:");
        result.AppendLine();

        foreach (var departure in departures.Take(20)) // Limit to 20 departures
        {
            result.AppendLine($"{departure.DepartureTime:hh\\:mm} - Route {departure.RouteShortName}");
            if (!string.IsNullOrEmpty(departure.Headsign))
            {
                result.AppendLine($"  → {departure.Headsign}");
            }
        }

        if (departures.Count == 0)
        {
            result.AppendLine("No departures found for the specified criteria.");
        }
        else if (departures.Count > 20)
        {
            result.AppendLine($"... and {departures.Count - 20} more departures");
        }

        return Task.FromResult(new MCPToolResult
        {
            Content = new List<MCPContent>
            {
                new MCPContent
                {
                    Type = "text",
                    Text = result.ToString()
                }
            }
        });
    }

    /// <summary>
    /// List all available routes
    /// </summary>
    private Task<MCPToolResult> ListRoutesAsync(Dictionary<string, object>? arguments)
    {
        var routes = _gtfsService.GetAllRoutes();
        
        var result = new StringBuilder();
        result.AppendLine($"Available routes ({routes.Count}):");
        result.AppendLine();

        foreach (var route in routes)
        {
            var routeName = !string.IsNullOrEmpty(route.RouteShortName) 
                ? route.RouteShortName 
                : route.RouteLongName ?? route.RouteId;
            
            result.AppendLine($"• {routeName}");
            if (!string.IsNullOrEmpty(route.RouteLongName) && route.RouteShortName != route.RouteLongName)
            {
                result.AppendLine($"  {route.RouteLongName}");
            }
            if (!string.IsNullOrEmpty(route.RouteDescription))
            {
                result.AppendLine($"  {route.RouteDescription}");
            }
        }

        return Task.FromResult(new MCPToolResult
        {
            Content = new List<MCPContent>
            {
                new MCPContent
                {
                    Type = "text",
                    Text = result.ToString()
                }
            }
        });
    }

    /// <summary>
    /// Create available tools for the MCP server
    /// </summary>
    private Dictionary<string, MCPTool> CreateTools()
    {
        return new Dictionary<string, MCPTool>
        {
            ["find_train_connections"] = new MCPTool
            {
                Name = "find_train_connections",
                Description = "Find train connections between two stops on a specific date",
                InputSchema = new MCPToolInputSchema
                {
                    Properties = new Dictionary<string, MCPToolProperty>
                    {
                        ["from_stop_id"] = new MCPToolProperty
                        {
                            Type = "string",
                            Description = "ID of the departure stop"
                        },
                        ["to_stop_id"] = new MCPToolProperty
                        {
                            Type = "string",
                            Description = "ID of the arrival stop"
                        },
                        ["date"] = new MCPToolProperty
                        {
                            Type = "string",
                            Description = "Date to search for connections (YYYY-MM-DD)",
                            Format = "date"
                        },
                        ["departure_time"] = new MCPToolProperty
                        {
                            Type = "string",
                            Description = "Optional earliest departure time (HH:MM)",
                            Format = "time"
                        }
                    },
                    Required = new List<string> { "from_stop_id", "to_stop_id", "date" }
                }
            },
            
            ["search_stops"] = new MCPTool
            {
                Name = "search_stops",
                Description = "Search for stops by name",
                InputSchema = new MCPToolInputSchema
                {
                    Properties = new Dictionary<string, MCPToolProperty>
                    {
                        ["search_term"] = new MCPToolProperty
                        {
                            Type = "string",
                            Description = "Search term to find stops"
                        }
                    },
                    Required = new List<string> { "search_term" }
                }
            },
            
            ["get_departures"] = new MCPTool
            {
                Name = "get_departures",
                Description = "Get departures from a specific stop on a given date",
                InputSchema = new MCPToolInputSchema
                {
                    Properties = new Dictionary<string, MCPToolProperty>
                    {
                        ["stop_id"] = new MCPToolProperty
                        {
                            Type = "string",
                            Description = "ID of the stop"
                        },
                        ["date"] = new MCPToolProperty
                        {
                            Type = "string",
                            Description = "Date to get departures for (YYYY-MM-DD)",
                            Format = "date"
                        },
                        ["from_time"] = new MCPToolProperty
                        {
                            Type = "string",
                            Description = "Optional earliest departure time (HH:MM)",
                            Format = "time"
                        }
                    },
                    Required = new List<string> { "stop_id", "date" }
                }
            },
            
            ["list_routes"] = new MCPTool
            {
                Name = "list_routes",
                Description = "List all available routes",
                InputSchema = new MCPToolInputSchema
                {
                    Properties = new Dictionary<string, MCPToolProperty>(),
                    Required = new List<string>()
                }
            }
        };
    }

    /// <summary>
    /// Process a single MCP request (for web mode)
    /// </summary>
    public async Task<string> ProcessRequestAsync(string jsonRequest)
    {
        try
        {
            var request = JsonConvert.DeserializeObject<MCPRequest>(jsonRequest);
            if (request == null)
            {
                throw new InvalidOperationException("Invalid request format");
            }

            var response = await HandleRequestAsync(request);
            return JsonConvert.SerializeObject(response, Formatting.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request: {Request}", jsonRequest);
            var errorResponse = new MCPResponse
            {
                Id = "error",
                Error = new MCPError
                {
                    Code = -32603,
                    Message = "Internal error",
                    Data = ex.Message
                }
            };
            return JsonConvert.SerializeObject(errorResponse, Formatting.None);
        }
    }
}
