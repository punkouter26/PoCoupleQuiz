# PoCoupleQuiz - Azure Setup

## Prerequisites

- [Azure Developer CLI (azd)](https://aka.ms/install-azd)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) for local development

## Local Development Setup

### 1. Install Azurite

```pwsh
npm install -g azurite
```

### 2. Start Azurite

```pwsh
azurite --silent --location ./AzuriteConfig --debug ./AzuriteConfig/debug.log
```

Azurite will start on default ports:
- Blob service: `http://127.0.0.1:10000`
- Queue service: `http://127.0.0.1:10001`
- Table service: `http://127.0.0.1:10002`

### 3. Configure Azure OpenAI (Optional)

For local development, the app will use a mock question service if Azure OpenAI is not configured.

To use real Azure OpenAI, update `appsettings.Development.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://posharedopenaieastus.openai.azure.com/",
    "Key": "your-actual-key-here",
    "DeploymentName": "gpt-35-turbo"
  }
}
```

### 4. Run the Application

```pwsh
dotnet run --project PoCoupleQuiz.Server
```

Access the application at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

## Azure Deployment

### Prerequisites

1. Azure subscription
2. Existing **PoShared** resource group with **PoSharedAppServicePlan** (Free F1 tier)
3. Access to shared Azure OpenAI resource at `https://posharedopenaieastus.openai.azure.com/`

### Deploy to Azure

```pwsh
# Initialize AZD environment (first time only)
azd init

# Set environment variables
azd env set AZURE_LOCATION eastus2

# Deploy infrastructure and application
azd up
```

The deployment will create:
- Resource Group: `rg-pocouplequiz-{env-name}`
- App Service: `app-pocouplequiz-{env-name}`
- Storage Account: `stpcq{env-name}` (with Tables)
- Application Insights: `appi-pocouplequiz-{env-name}`
- Log Analytics Workspace: `log-pocouplequiz-{env-name}`

### Post-Deployment Configuration

After deployment, you need to manually set the Azure OpenAI API key:

```pwsh
# Get the App Service name from deployment outputs
$appName = azd env get-values | Select-String "AZURE_APP_SERVICE_NAME" | ForEach-Object { $_.ToString().Split("=")[1].Trim('"') }

# Set the Azure OpenAI Key
az webapp config appsettings set --name $appName --resource-group rg-pocouplequiz-{env-name} --settings AzureOpenAI__Key="your-actual-openai-key"
```

## Resource Naming Convention

All resources follow the naming pattern: `{resource-type}-pocouplequiz-{environment}`

- Resource Group: `rg-pocouplequiz-{env}`
- App Service: `app-pocouplequiz-{env}`
- Storage Account: `stpcq{env}` (no hyphens allowed)
- Application Insights: `appi-pocouplequiz-{env}`
- Log Analytics: `log-pocouplequiz-{env}`

## Clean Up Resources

To delete all Azure resources:

```pwsh
azd down
```

## Troubleshooting

### PoSharedAppServicePlan Not Found

If deployment fails because the shared App Service Plan doesn't exist:

1. Create the PoShared resource group:
   ```pwsh
   az group create --name PoShared --location eastus2
   ```

2. Create the App Service Plan:
   ```pwsh
   az appservice plan create --name PoSharedAppServicePlan --resource-group PoShared --sku F1 --location eastus2
   ```

3. Re-run `azd up`

### Azurite Connection Issues

Ensure Azurite is running before starting the application locally:

```pwsh
# Check if Azurite is running
Get-Process azurite -ErrorAction SilentlyContinue
```

If not running, start it:

```pwsh
azurite --silent --location ./AzuriteConfig --debug ./AzuriteConfig/debug.log
```
