<#
.SYNOPSIS
    Configures .NET User Secrets for local development using Azure resources.

.DESCRIPTION
    This script retrieves values from the azd environment and configures
    .NET User Secrets for the PoCoupleQuiz.Server project. It sets up:
    - Application Insights connection string
    - Azure Table Storage connection string (Azurite)
    - Azure OpenAI endpoint and deployment name
    
    The Azure OpenAI API key must be set manually for security.

.PARAMETER OpenAIApiKey
    The Azure OpenAI API key. If not provided, you'll be prompted to enter it.

.EXAMPLE
    .\Configure-LocalSecrets.ps1
    
.EXAMPLE
    .\Configure-LocalSecrets.ps1 -OpenAIApiKey "sk-your-key-here"

.NOTES
    Prerequisites:
    - Azure Developer CLI (azd) must be installed
    - Must have run 'azd provision' successfully
    - Must be run from the repository root or infra folder
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$OpenAIApiKey
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PoCoupleQuiz - Local Secrets Configuration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if azd is installed
try {
    $azdVersion = azd version 2>&1
    Write-Host "✓ Azure Developer CLI is installed" -ForegroundColor Green
} catch {
    Write-Host "✗ Azure Developer CLI (azd) is not installed" -ForegroundColor Red
    Write-Host "  Install from: https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd" -ForegroundColor Yellow
    exit 1
}

# Navigate to repository root if in infra folder
if (Test-Path ".\infra") {
    $repoRoot = Get-Location
} elseif ((Get-Location).Path -like "*\infra") {
    Set-Location ..
    $repoRoot = Get-Location
} else {
    Write-Host "✗ Script must be run from repository root or infra folder" -ForegroundColor Red
    exit 1
}

Write-Host "Repository root: $repoRoot" -ForegroundColor Gray
Write-Host ""

# Check if azd environment exists
try {
    $envValues = azd env get-values 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ No azd environment found. Run 'azd provision' first." -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ azd environment found" -ForegroundColor Green
} catch {
    Write-Host "✗ Error reading azd environment" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Retrieving Azure resource values..." -ForegroundColor Cyan

# Parse azd env get-values output
$envHash = @{}
$envValues -split "`n" | ForEach-Object {
    if ($_ -match '^([^=]+)=(.*)$') {
        $key = $matches[1]
        $value = $matches[2] -replace '^"', '' -replace '"$', ''
        $envHash[$key] = $value
    }
}

# Extract required values
$appInsightsConnectionString = $envHash["APPLICATIONINSIGHTS_CONNECTION_STRING"]
$openAIEndpoint = $envHash["AZURE_OPENAI_ENDPOINT"]
$openAIDeploymentName = $envHash["AZURE_OPENAI_DEPLOYMENT_NAME"]
$storageConnectionString = $envHash["AZURE_STORAGE_CONNECTION_STRING"]

# Validate required values exist
$missingValues = @()
if (-not $appInsightsConnectionString) { $missingValues += "APPLICATIONINSIGHTS_CONNECTION_STRING" }
if (-not $openAIEndpoint) { $missingValues += "AZURE_OPENAI_ENDPOINT" }
if (-not $openAIDeploymentName) { $missingValues += "AZURE_OPENAI_DEPLOYMENT_NAME" }

if ($missingValues.Count -gt 0) {
    Write-Host "✗ Missing required values from azd environment:" -ForegroundColor Red
    $missingValues | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
    Write-Host ""
    Write-Host "Run 'azd provision' to create Azure resources." -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Retrieved Azure resource values" -ForegroundColor Green
Write-Host ""

# Navigate to Server project
$serverProject = Join-Path $repoRoot "PoCoupleQuiz.Server"
if (-not (Test-Path $serverProject)) {
    Write-Host "✗ Server project not found at: $serverProject" -ForegroundColor Red
    exit 1
}

Set-Location $serverProject
Write-Host "Configuring user secrets for: PoCoupleQuiz.Server" -ForegroundColor Cyan
Write-Host ""

# Function to set user secret
function Set-UserSecret {
    param(
        [string]$Key,
        [string]$Value,
        [string]$DisplayValue = $Value
    )
    
    try {
        dotnet user-secrets set $Key $Value 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ $Key" -ForegroundColor Green
            if ($DisplayValue.Length -gt 50) {
                Write-Host "    = $($DisplayValue.Substring(0, 47))..." -ForegroundColor Gray
            } else {
                Write-Host "    = $DisplayValue" -ForegroundColor Gray
            }
        } else {
            Write-Host "  ✗ Failed to set: $Key" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "  ✗ Error setting $Key : $_" -ForegroundColor Red
        return $false
    }
    return $true
}

# Set Application Insights
Write-Host "Setting Application Insights..." -ForegroundColor Cyan
$success = Set-UserSecret "ApplicationInsights:ConnectionString" $appInsightsConnectionString "InstrumentationKey=****"
Write-Host ""

# Set Azure Table Storage (Azurite)
Write-Host "Setting Azure Table Storage (Azurite)..." -ForegroundColor Cyan
$success = $success -and (Set-UserSecret "AzureTableStorage:ConnectionString" "UseDevelopmentStorage=true")
Write-Host ""

# Set Azure OpenAI Endpoint
Write-Host "Setting Azure OpenAI configuration..." -ForegroundColor Cyan
$success = $success -and (Set-UserSecret "AzureOpenAI:Endpoint" $openAIEndpoint)
$success = $success -and (Set-UserSecret "AzureOpenAI:DeploymentName" $openAIDeploymentName)

# Set Azure OpenAI API Key
if (-not $OpenAIApiKey) {
    Write-Host ""
    Write-Host "⚠ Azure OpenAI API Key is required" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To get your API key:" -ForegroundColor Gray
    Write-Host "  1. Go to Azure Portal" -ForegroundColor Gray
    Write-Host "  2. Navigate to: PoShared resource group" -ForegroundColor Gray
    Write-Host "  3. Open: posharedopenaieastus" -ForegroundColor Gray
    Write-Host "  4. Go to: Keys and Endpoint" -ForegroundColor Gray
    Write-Host "  5. Copy: KEY 1 or KEY 2" -ForegroundColor Gray
    Write-Host ""
    
    $OpenAIApiKey = Read-Host "Enter Azure OpenAI API Key (or press Enter to skip)"
}

if ($OpenAIApiKey) {
    $success = $success -and (Set-UserSecret "AzureOpenAI:ApiKey" $OpenAIApiKey "sk-****")
    Write-Host ""
} else {
    Write-Host "  ⚠ Skipped: AzureOpenAI:ApiKey (must be set manually)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Set it later with:" -ForegroundColor Gray
    Write-Host "    dotnet user-secrets set `"AzureOpenAI:ApiKey`" `"your-key-here`"" -ForegroundColor Gray
    Write-Host ""
}

# Verify secrets
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Configured User Secrets:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    $secrets = dotnet user-secrets list 2>&1
    if ($LASTEXITCODE -eq 0) {
        $secrets | ForEach-Object {
            if ($_ -match 'ApiKey') {
                Write-Host $_ -replace '= .+', '= ****' -ForegroundColor Gray
            } elseif ($_ -match 'ConnectionString') {
                Write-Host $_ -replace '= .+', '= ****' -ForegroundColor Gray
            } else {
                Write-Host $_ -ForegroundColor Gray
            }
        }
    }
} catch {
    Write-Host "Unable to list user secrets" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Start Azurite (in a separate terminal):" -ForegroundColor White
Write-Host "   azurite --silent --location c:\azurite --debug c:\azurite\debug.log" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Run the application:" -ForegroundColor White
Write-Host "   cd PoCoupleQuiz.Server" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Verify health status:" -ForegroundColor White
Write-Host "   Navigate to: https://localhost:[port]/diag" -ForegroundColor Gray
Write-Host ""
Write-Host "4. View telemetry in Azure Portal:" -ForegroundColor White
Write-Host "   Resource Group: PoCoupleQuiz -> appi-PoCoupleQuiz" -ForegroundColor Gray
Write-Host ""

if (-not $OpenAIApiKey) {
    Write-Host "⚠ Remember to set the Azure OpenAI API key manually!" -ForegroundColor Yellow
    Write-Host ""
}

Set-Location $repoRoot
Write-Host "✓ Configuration complete!" -ForegroundColor Green
