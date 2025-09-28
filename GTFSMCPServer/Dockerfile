# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GTFSMCPServer.csproj", "./"]
RUN dotnet restore "GTFSMCPServer.csproj"
COPY . .
RUN dotnet build "GTFSMCPServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GTFSMCPServer.csproj" -c Release -o /app/publish

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy GTFS data
COPY Data/ ./Data/

# Set environment variables
ENV GTFS_DATA_PATH=/app/Data
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "GTFSMCPServer.dll"]
