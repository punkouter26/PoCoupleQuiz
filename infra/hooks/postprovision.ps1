#!/usr/bin/env pwsh

# Post-provision hook for AZD
# This script runs after Azure resources are provisioned

param(
    [string]$AZURE_ENV_NAME,
    [string]$AZURE_LOCATION
)

Write-Host "Running post-provision configuration..." -ForegroundColor Green

# Get the resource group name
$resourceGroup = "rg-pocouplequiz-$AZURE_ENV_NAME"

# Get the app service name
$appServiceName = az resource list --resource-group $resourceGroup --resource-type "Microsoft.Web/sites" --query "[0].name" -o tsv

if ($appServiceName) {
    Write-Host "Found App Service: $appServiceName" -ForegroundColor Cyan
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Yellow
    Write-Host "IMPORTANT: Manual Configuration Required" -ForegroundColor Yellow
    Write-Host "================================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "You need to set the Azure OpenAI API key manually:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Run this command:" -ForegroundColor White
    Write-Host "az webapp config appsettings set ``" -ForegroundColor Cyan
    Write-Host "  --name $appServiceName ``" -ForegroundColor Cyan
    Write-Host "  --resource-group $resourceGroup ``" -ForegroundColor Cyan
    Write-Host "  --settings AzureOpenAI__Key=`"YOUR_ACTUAL_OPENAI_KEY`"" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "Post-provision configuration complete!" -ForegroundColor Green
