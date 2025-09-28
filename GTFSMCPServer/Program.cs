using GTFSMCPServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace GTFSMCPServer;

class Program
{
    static async Task Main(string[] args)
    {
        // Check if running in Azure Container Apps (has PORT environment variable)
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        var isWebMode = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")) || 
                       !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CONTAINER_APP_NAME")) ||
                       args.Contains("--web");

        if (isWebMode)
        {
            await RunWebServer(args);
        }
        else
        {
            await RunMCPServer(args);
        }
    }

    static async Task RunWebServer(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services
        builder.Services.AddSingleton<GTFSDataService>();
        builder.Services.AddSingleton<MCPServer>();

        var app = builder.Build();

        // Get GTFS data path
        string? gtfsDataPath = args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable("GTFS_DATA_PATH");
        
        // Initialize GTFS data
        var gtfsService = app.Services.GetRequiredService<GTFSDataService>();
        if (!string.IsNullOrEmpty(gtfsDataPath))
        {
            await gtfsService.LoadDataAsync(gtfsDataPath);
        }

        // Health check endpoint
        app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

        // MCP endpoint for web-based requests
        app.MapPost("/mcp", async (HttpRequest request, MCPServer mcpServer) =>
        {
            try
            {
                using var reader = new StreamReader(request.Body);
                var jsonRequest = await reader.ReadToEndAsync();
                
                // Process the MCP request
                var response = await mcpServer.ProcessRequestAsync(jsonRequest);
                
                return Results.Content(response, "application/json");
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // Info endpoint
        app.MapGet("/", () => Results.Ok(new { 
            name = "GTFS MCP Server", 
            version = "1.0.0",
            endpoints = new[] { "/health", "/mcp" },
            description = "Model Context Protocol server for GTFS transit data"
        }));

        await app.RunAsync();
    }

    static async Task RunMCPServer(string[] args)
    {
        // Create host builder
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<GTFSDataService>();
                services.AddSingleton<MCPServer>();
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            })
            .UseConsoleLifetime();

        var host = hostBuilder.Build();
        
        // Get required services
        var mcpServer = host.Services.GetRequiredService<MCPServer>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting GTFS MCP Server...");
            
            // Get GTFS data path from command line arguments or environment variable
            string? gtfsDataPath = args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable("GTFS_DATA_PATH");
            
            if (!string.IsNullOrEmpty(gtfsDataPath))
            {
                logger.LogInformation("GTFS data path: {Path}", gtfsDataPath);
            }
            else
            {
                logger.LogWarning("No GTFS data path provided. Use command line argument or set GTFS_DATA_PATH environment variable.");
            }

            // Start the MCP server
            await mcpServer.StartAsync(gtfsDataPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in GTFS MCP Server");
            Environment.Exit(1);
        }
    }
}
