# Azure Deployment Quick Reference

## One-Command Deployment

```pwsh
azd up
```

This single command will:
1. Provision all Azure resources using Bicep
2. Build the .NET application
3. Deploy to Azure App Service

## Resource Created

The deployment creates a resource group named `rg-pocouplequiz-{environment}` containing:

| Resource Type | Name Pattern | Purpose |
|--------------|--------------|---------|
| Resource Group | `rg-pocouplequiz-{env}` | Container for all resources |
| App Service | `app-pocouplequiz-{env}` | Hosts the Blazor web application |
| Storage Account | `stpcq{env}` | Azure Table Storage for game data |
| Table: Teams | `PoCoupleQuizTeams` | Stores team information |
| Table: GameHistory | `PoCoupleQuizGameHistory` | Stores game history |
| Application Insights | `appi-pocouplequiz-{env}` | Application telemetry |
| Log Analytics Workspace | `log-pocouplequiz-{env}` | Log storage for App Insights |

## Prerequisites

✅ **Existing Shared Resources** (in PoShared resource group):
- App Service Plan: `PoSharedAppServicePlan` (Free F1 tier)
- Azure OpenAI: `posharedopenaieastus` with `gpt-35-turbo` deployment

⚠️ **If the shared App Service Plan doesn't exist**, deployment will fail with:
```
Resource 'PoSharedAppServicePlan' not found in resource group 'PoShared'
```

**Fix:** Create the shared resources:
```pwsh
az group create --name PoShared --location eastus2
az appservice plan create --name PoSharedAppServicePlan --resource-group PoShared --sku F1
```

## Environment Configuration

Default values (set by AZD automatically):
- **Location**: `eastus2`
- **Environment Name**: Prompted during `azd up`

## Post-Deployment Steps

### 1. Set Azure OpenAI Key

The Azure OpenAI API key must be set manually after deployment:

```pwsh
# Get app service name
$appName = azd env get-values | Select-String "AZURE_APP_SERVICE_NAME" | ForEach-Object { $_.ToString().Split("=")[1].Trim('"') }

# Get resource group
$rg = azd env get-values | Select-String "AZURE_RESOURCE_GROUP" | ForEach-Object { $_.ToString().Split("=")[1].Trim('"') }

# Set the Azure OpenAI Key
az webapp config appsettings set `
  --name $appName `
  --resource-group $rg `
  --settings AzureOpenAI__Key="your-actual-openai-key-here"
```

### 2. Verify Deployment

```pwsh
# Get the app URL
azd env get-values | Select-String "AZURE_APP_SERVICE_URI"

# Open in browser
$uri = azd env get-values | Select-String "AZURE_APP_SERVICE_URI" | ForEach-Object { $_.ToString().Split("=")[1].Trim('"') }
Start-Process $uri
```

### 3. View Health Status

Navigate to `/diag` to see the health status of all dependencies:
```
https://app-pocouplequiz-{env}.azurewebsites.net/diag
```

## Local Development

### Start Azurite (Local Storage Emulator)

```pwsh
.\start-azurite.ps1
```

Or manually:
```pwsh
azurite --silent --location ./AzuriteConfig --debug ./AzuriteConfig/debug.log
```

### Run Locally

```pwsh
dotnet run --project PoCoupleQuiz.Server
```

Application will be available at:
- http://localhost:5000
- https://localhost:5001

## Tear Down

Remove all Azure resources:

```pwsh
azd down
```

## Troubleshooting

### Build Fails

Ensure .NET 9.0 SDK is installed:
```pwsh
dotnet --version  # Should show 9.x.x
```

### Deployment Fails - PoSharedAppServicePlan Not Found

Create the shared App Service Plan:
```pwsh
az group create --name PoShared --location eastus2
az appservice plan create --name PoSharedAppServicePlan --resource-group PoShared --sku F1
```

### Storage Connection Issues

Check if Azurite is running:
```pwsh
Get-Process azurite -ErrorAction SilentlyContinue
```

### View Deployment Logs

```pwsh
azd deploy --debug
```

## Cost Estimate

Using all free/cheapest tiers:

- App Service Plan: **$0/month** (Shared F1 tier, already exists)
- Storage Account: **~$0.02/month** (Standard LRS, pay-as-you-go)
- Application Insights: **First 5GB free/month**
- Log Analytics: **First 5GB free/month**

**Estimated Total**: < $1/month for typical development usage
