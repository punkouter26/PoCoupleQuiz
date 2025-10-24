# Start Azurite for local development
# This script starts Azurite with the correct configuration for PoCoupleQuiz

Write-Host "Starting Azurite for PoCoupleQuiz local development..." -ForegroundColor Green

# Check if Azurite is installed
$azuriteInstalled = Get-Command azurite -ErrorAction SilentlyContinue

if (-not $azuriteInstalled) {
    Write-Host "Azurite is not installed. Installing..." -ForegroundColor Yellow
    npm install -g azurite
}

# Create Azurite config directory if it doesn't exist
$azuriteConfigPath = Join-Path $PSScriptRoot "AzuriteConfig"
if (-not (Test-Path $azuriteConfigPath)) {
    New-Item -ItemType Directory -Path $azuriteConfigPath -Force | Out-Null
    Write-Host "Created Azurite configuration directory: $azuriteConfigPath" -ForegroundColor Cyan
}

# Start Azurite
Write-Host "Starting Azurite on default ports..." -ForegroundColor Cyan
Write-Host "  - Blob service: http://127.0.0.1:10000" -ForegroundColor Gray
Write-Host "  - Queue service: http://127.0.0.1:10001" -ForegroundColor Gray
Write-Host "  - Table service: http://127.0.0.1:10002" -ForegroundColor Gray

azurite --silent --location $azuriteConfigPath --debug (Join-Path $azuriteConfigPath "debug.log")
