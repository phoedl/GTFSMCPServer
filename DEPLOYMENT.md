# Deploying GTFS MCP Server to Azure

This guide explains how to deploy your GTFS MCP Server to Azure using Azure Developer CLI (azd) and Azure Container Apps.

## Prerequisites

1. **Azure Developer CLI (azd)** - Install from https://aka.ms/azd-install
2. **Azure CLI** - Install from https://docs.microsoft.com/cli/azure/install-azure-cli
3. **Docker** - Install from https://docs.docker.com/get-docker/
4. **Azure subscription with appropriate permissions**

## Deployment Steps

### 1. Initialize the Azure Developer Environment

```powershell
# Navigate to your project root
cd E:\Source\MCP\Test

# Initialize azd (if not already done)
azd init

# Log in to Azure
azd auth login
```

### 2. Set Environment Variables

```powershell
# Set your environment name (must be globally unique)
azd env set AZURE_ENV_NAME "gtfs-mcp-prod-001"

# Set your preferred Azure region
azd env set AZURE_LOCATION "eastus2"
```

### 3. Preview the Deployment

```powershell
# Preview what resources will be created
azd provision --preview
```

### 4. Deploy to Azure

```powershell
# Deploy infrastructure and application
azd up
```

This command will:
- Create a resource group
- Set up Azure Container Registry
- Create Container Apps Environment with Log Analytics
- Deploy your application as a container
- Set up Application Insights for monitoring

### 5. Verify the Deployment

After deployment completes, azd will provide you with:
- **Container App URL**: Your web endpoint for HTTP requests
- **Resource Group**: Where all resources are created
- **Container Registry**: Where your Docker image is stored

## Usage After Deployment

### Web Mode (HTTP API)
Your deployed server will be accessible via HTTP with these endpoints:

- `GET /` - Server information
- `GET /health` - Health check
- `POST /mcp` - MCP protocol requests

Example usage:
```bash
# Health check
curl https://your-app-url.azurecontainerapps.io/health

# Send MCP request
curl -X POST https://your-app-url.azurecontainerapps.io/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc": "2.0", "id": 1, "method": "tools/list", "params": {}}'
```

### MCP Client Integration
You can integrate with MCP clients by pointing them to your deployed URL's `/mcp` endpoint.

## Configuration

### Environment Variables
- `GTFS_DATA_PATH`: Path to GTFS data files (defaults to `/app/Data`)
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: Auto-configured for monitoring

### GTFS Data
Replace the sample data in the `Data/` folder with your actual GTFS files:
- `agency.txt`
- `stops.txt` 
- `routes.txt`
- `trips.txt`
- `stop_times.txt`
- `calendar.txt`

## Monitoring and Logs

### View Application Logs
```powershell
# View recent logs
azd logs

# Stream logs in real-time
azd logs --follow
```

### Azure Portal
1. Go to the Azure Portal
2. Navigate to your resource group
3. Click on the Container App
4. View logs, metrics, and configuration

## Updating the Deployment

```powershell
# Redeploy after making changes
azd deploy

# Or rebuild and redeploy everything
azd up
```

## Cleanup

```powershell
# Remove all Azure resources
azd down
```

## Cost Considerations

- **Container Apps**: Pay per vCPU-second and memory GB-second
- **Container Registry**: Storage costs for Docker images
- **Log Analytics**: Data ingestion and retention costs
- **Application Insights**: Telemetry data costs

Estimated cost: ~$10-50/month depending on usage patterns.

## Troubleshooting

### Build Issues
If you encounter build issues due to running processes:
```powershell
# Kill any running GTFSMCPServer processes
Get-Process -Name "GTFSMCPServer" -ErrorAction SilentlyContinue | Stop-Process -Force

# Then rebuild
dotnet clean
dotnet build
```

### Deployment Issues
- Check Azure quotas and region availability
- Verify Docker is running locally
- Ensure unique environment names
- Check Azure permissions

## Security Best Practices

- The deployment uses managed identity for secure access to Azure Container Registry
- CORS is configured for web access
- Application Insights provides monitoring without exposing sensitive data
- No hardcoded credentials are used

## Next Steps

1. Replace sample GTFS data with real transit data
2. Set up CI/CD pipeline for automated deployments
3. Configure custom domain and SSL certificate
4. Implement authentication if needed for public access
5. Set up alerts and monitoring dashboards
