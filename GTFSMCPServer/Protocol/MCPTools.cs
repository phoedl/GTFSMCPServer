using Newtonsoft.Json;

namespace GTFSMCPServer.Protocol;

/// <summary>
/// MCP Tool definition
/// </summary>
public class MCPTool
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("inputSchema")]
    public MCPToolInputSchema InputSchema { get; set; } = new();
}

/// <summary>
/// JSON Schema for MCP Tool input parameters
/// </summary>
public class MCPToolInputSchema
{
    [JsonProperty("type")]
    public string Type { get; set; } = "object";

    [JsonProperty("properties")]
    public Dictionary<string, MCPToolProperty> Properties { get; set; } = new();

    [JsonProperty("required")]
    public List<string> Required { get; set; } = new();
}

/// <summary>
/// JSON Schema property definition
/// </summary>
public class MCPToolProperty
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("enum")]
    public List<string>? Enum { get; set; }

    [JsonProperty("format")]
    public string? Format { get; set; }
}

/// <summary>
/// MCP Tool call request
/// </summary>
public class MCPToolCall
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("arguments")]
    public Dictionary<string, object>? Arguments { get; set; }
}

/// <summary>
/// MCP Tool call result
/// </summary>
public class MCPToolResult
{
    [JsonProperty("content")]
    public List<MCPContent> Content { get; set; } = new();

    [JsonProperty("isError")]
    public bool IsError { get; set; }
}

/// <summary>
/// MCP Content object
/// </summary>
public class MCPContent
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("text")]
    public string? Text { get; set; }
}

/// <summary>
/// Server capabilities
/// </summary>
public class MCPServerCapabilities
{
    [JsonProperty("tools")]
    public MCPToolsCapability? Tools { get; set; }
}

public class MCPToolsCapability
{
    [JsonProperty("listChanged")]
    public bool ListChanged { get; set; } = false;
}

/// <summary>
/// Initialize request/response
/// </summary>
public class MCPInitializeParams
{
    [JsonProperty("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonProperty("capabilities")]
    public MCPClientCapabilities Capabilities { get; set; } = new();

    [JsonProperty("clientInfo")]
    public MCPClientInfo ClientInfo { get; set; } = new();
}

public class MCPClientCapabilities
{
    [JsonProperty("tools")]
    public object? Tools { get; set; }
}

public class MCPClientInfo
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;
}

public class MCPInitializeResult
{
    [JsonProperty("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonProperty("capabilities")]
    public MCPServerCapabilities Capabilities { get; set; } = new();

    [JsonProperty("serverInfo")]
    public MCPServerInfo ServerInfo { get; set; } = new();
}

public class MCPServerInfo
{
    [JsonProperty("name")]
    public string Name { get; set; } = "GTFS MCP Server";

    [JsonProperty("version")]
    public string Version { get; set; } = "1.0.0";
}
