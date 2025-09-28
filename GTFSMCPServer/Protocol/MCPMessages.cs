using Newtonsoft.Json;

namespace GTFSMCPServer.Protocol;

/// <summary>
/// Base class for all MCP messages
/// </summary>
public abstract class MCPMessage
{
    [JsonProperty("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
}

/// <summary>
/// MCP Request message
/// </summary>
public class MCPRequest : MCPMessage
{
    [JsonProperty("id")]
    public object Id { get; set; } = null!;

    [JsonProperty("method")]
    public string Method { get; set; } = string.Empty;

    [JsonProperty("params")]
    public object? Params { get; set; }
}

/// <summary>
/// MCP Response message
/// </summary>
public class MCPResponse : MCPMessage
{
    [JsonProperty("id")]
    public object Id { get; set; } = null!;

    [JsonProperty("result")]
    public object? Result { get; set; }

    [JsonProperty("error")]
    public MCPError? Error { get; set; }
}

/// <summary>
/// MCP Error object
/// </summary>
public class MCPError
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("data")]
    public object? Data { get; set; }
}

/// <summary>
/// MCP Notification message
/// </summary>
public class MCPNotification : MCPMessage
{
    [JsonProperty("method")]
    public string Method { get; set; } = string.Empty;

    [JsonProperty("params")]
    public object? Params { get; set; }
}
