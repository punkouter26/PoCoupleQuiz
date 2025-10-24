@description('The location used for all deployed resources')
param location string = resourceGroup().location

@description('Tags that will be applied to all resources')
param tags object = {}

// Base name for all resources
var baseName = 'PoCoupleQuiz'

// Storage account name (must be 3-24 characters, lowercase, no hyphens)
var storageAccountName = 'stpocouplequiz'

// Shared resource group for App Service Plan - using hardcoded resource ID for cross-RG reference
var sharedAppServicePlanId = '/subscriptions/${subscription().subscriptionId}/resourceGroups/PoShared/providers/Microsoft.Web/serverfarms/PoShared'

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
  }
}

// Application Insights (for telemetry)
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
  sku: {
    name: 'Standard_LRS' // Cheapest option: Locally redundant storage
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

// Table Service (part of Storage Account)
resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

// Create tables for the application
resource teamsTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
  parent: tableService
  name: 'PoCoupleQuizTeams'
}

resource gameHistoryTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
  parent: tableService
  name: 'PoCoupleQuizGameHistory'
}

// App Service for hosting the Blazor application
resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: baseName
  location: 'eastus2' // Must match App Service Plan location (PoShared is in East US 2)
  tags: union(tags, { 'azd-service-name': 'web' })
  kind: 'app'
  properties: {
    serverFarmId: sharedAppServicePlanId
    httpsOnly: true
    siteConfig: {
      windowsFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: false // Required for Free (F1) tier
      use32BitWorkerProcess: true // Required for Free (F1) tier
      netFrameworkVersion: 'v9.0'
      httpLoggingEnabled: true
      logsDirectorySizeLimit: 35
      requestTracingEnabled: true
      minTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
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
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'AzureStorage__ConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'AzureOpenAI__Endpoint'
          value: 'https://posharedopenaieastus.openai.azure.com/'
        }
        {
          name: 'AzureOpenAI__Key'
          value: '' // Must be set manually or via Key Vault
        }
        {
          name: 'AzureOpenAI__DeploymentName'
          value: 'gpt-35-turbo'
        }
        {
          name: 'GameSettings__KingAnswerTimeSeconds'
          value: '45'
        }
        {
          name: 'GameSettings__PlayerAnswerTimeEasySeconds'
          value: '40'
        }
        {
          name: 'GameSettings__PlayerAnswerTimeMediumSeconds'
          value: '30'
        }
        {
          name: 'GameSettings__PlayerAnswerTimeHardSeconds'
          value: '20'
        }
      ]
    }
  }
}

// Outputs
output APP_SERVICE_NAME string = appService.name
output APP_SERVICE_URI string = 'https://${appService.properties.defaultHostName}'
output STORAGE_ACCOUNT_NAME string = storageAccount.name
output APPLICATION_INSIGHTS_NAME string = applicationInsights.name
output APPLICATION_INSIGHTS_CONNECTION_STRING string = applicationInsights.properties.ConnectionString
output OPENAI_ENDPOINT string = 'https://posharedopenaieastus.openai.azure.com/'
output LOG_ANALYTICS_WORKSPACE_NAME string = logAnalyticsWorkspace.name
