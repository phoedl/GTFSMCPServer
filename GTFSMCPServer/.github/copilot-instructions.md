# GitHub Copilot Instructions

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

This is an MCP (Model Context Protocol) Server project written in C# that provides functionality for querying train connection data in GTFS (General Transit Feed Specification) format.

## Key Information:
- You can find more info and examples at https://modelcontextprotocol.io/llms-full.txt
- The project uses .NET 8 and handles GTFS data parsing
- GTFS format includes CSV files: stops.txt, routes.txt, trips.txt, stop_times.txt, calendar.txt, etc.
- The server should expose tools for querying train schedules, routes, and connections
- Use proper async/await patterns for I/O operations
- Follow MCP protocol specifications for message handling
- Include proper error handling and logging
