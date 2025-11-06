@description('The location used for all deployed resources')
param location string = resourceGroup().location

@description('Tags that will be applied to all resources')
param tags object = {}

@description('Service name for the App Service')
param serviceName string = 'web'

// Base name for all resources
var baseName = 'PoCoupleQuiz'
var abbrs = loadJsonContent('./abbreviations.json')

// Shared Azure OpenAI resource in PoShared resource group (existing resource)
var sharedOpenAIEndpoint = 'https://posharedopenaieastus.openai.azure.com/'
var sharedOpenAIDeploymentName = 'gpt-35-turbo'

// Shared App Service Plan in PoShared resource group (existing resource)
var sharedResourceGroupName = 'PoShared'
var sharedAppServicePlanName = 'PoShared' // F1 tier plan in East US 2

// Generate unique resource names
var appServiceName = '${abbrs.webSitesAppService}${baseName}'
var storageAccountName = toLower('${abbrs.storageStorageAccounts}${take(replace(baseName, '-', ''), 17)}')

// ============================================================================
// PRODUCTION INFRASTRUCTURE
// ============================================================================
// This Bicep file provisions all resources required for Azure deployment:
// 1. Log Analytics Workspace - Required for Application Insights
// 2. Application Insights - For telemetry and monitoring
// 3. Storage Account - For Azure Table Storage (game history, teams)
// 4. App Service Plan - Hosting infrastructure
// 5. App Service - Web application deployment
// ============================================================================

// Log Analytics Workspace (required for Application Insights)
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-${baseName}'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018' // Pay-as-you-go pricing tier
    }
    retentionInDays: 30 // Minimum retention period
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Application Insights (for telemetry and monitoring)
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${baseName}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Storage Account (for Azure Table Storage)
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS' // Locally redundant storage (lowest cost)
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    supportsHttpsTrafficOnly: true
    accessTier: 'Hot'
    encryption: {
      services: {
        blob: {
          enabled: true
        }
        file: {
          enabled: true
        }
        table: {
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
  }
}

// Table Service (for game history and team data)
resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-05-01' = {
  name: 'default'
  parent: storageAccount
}

// Reference to existing App Service Plan in PoShared resource group
resource existingAppServicePlan 'Microsoft.Web/serverfarms@2024-04-01' existing = {
  scope: resourceGroup(sharedResourceGroupName)
  name: sharedAppServicePlanName
}

// App Service (web application)
resource appService 'Microsoft.Web/sites@2024-04-01' = {
  name: appServiceName
  location: location
  tags: union(tags, {
    'azd-service-name': serviceName
  })
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: existingAppServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      alwaysOn: false // Free tier doesn't support Always On
      http20Enabled: true
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsights__ConnectionString'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'XDT_MicodeAgent_MODE'
          value: 'recommended'
        }
        {
          name: 'APPINSIGHTS_PROFILERFEATURE_VERSION'
          value: '1.0.0'  // Enable Application Insights Profiler
        }
        {
          name: 'APPINSIGHTS_SNAPSHOTFEATURE_VERSION'
          value: '1.0.0'  // Enable Snapshot Debugger
        }
        {
          name: 'DiagnosticServices_EXTENSION_VERSION'
          value: '~3'  // Required for Snapshot Debugger
        }
        {
          name: 'SnapshotDebugger_EXTENSION_VERSION'
          value: 'disabled'  // Change to 'latest' to enable (additional cost ~$0.26/GB)
        }
        {
          name: 'InstrumentationEngine_EXTENSION_VERSION'
          value: 'disabled'  // Change to 'latest' to enable profiler (minimal cost)
        }
        {
          name: 'AzureStorage__ConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'AzureOpenAI__Endpoint'
          value: sharedOpenAIEndpoint
        }
        {
          name: 'AzureOpenAI__DeploymentName'
          value: sharedOpenAIDeploymentName
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
      healthCheckPath: '/health/live'
      cors: {
        allowedOrigins: [
          'https://${appServiceName}.azurewebsites.net'
        ]
        supportCredentials: false
      }
    }
  }
}

// ============================================================================
// OUTPUTS FOR AZURE DEPLOYMENT
// ============================================================================

// Application Insights outputs
output APPLICATION_INSIGHTS_NAME string = applicationInsights.name
output APPLICATION_INSIGHTS_CONNECTION_STRING string = applicationInsights.properties.ConnectionString
output APPLICATION_INSIGHTS_INSTRUMENTATION_KEY string = applicationInsights.properties.InstrumentationKey

// Log Analytics outputs
output LOG_ANALYTICS_WORKSPACE_NAME string = logAnalyticsWorkspace.name
output LOG_ANALYTICS_WORKSPACE_ID string = logAnalyticsWorkspace.id

// Azure OpenAI outputs (shared resource)
output OPENAI_ENDPOINT string = sharedOpenAIEndpoint
output OPENAI_DEPLOYMENT_NAME string = sharedOpenAIDeploymentName

// Storage Account outputs
output STORAGE_ACCOUNT_NAME string = storageAccount.name
// Note: Connection string with account key should not be output for security reasons
// Use Managed Identity or retrieve from App Service configuration

// App Service outputs
output APP_SERVICE_NAME string = appService.name
output APP_SERVICE_URL string = 'https://${appService.properties.defaultHostName}'

// Service-specific outputs (required by azd)
output SERVICE_WEB_NAME string = appService.name
output SERVICE_WEB_URI string = 'https://${appService.properties.defaultHostName}'
