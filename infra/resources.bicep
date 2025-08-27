@description('The location used for all deployed resources')
param location string = resourceGroup().location

@description('Tags that will be applied to all resources')
param tags object = {}

// Reference existing shared resources in PoShared resource group
var sharedResourceGroupName = 'PoShared'

// Reference to existing shared App Service Plan
resource existingAppServicePlan 'Microsoft.Web/serverfarms@2023-12-01' existing = {
  name: 'PoSharedAppServicePlan'
  scope: resourceGroup(sharedResourceGroupName)
}

// Reference to existing shared Application Insights
resource existingAppInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: 'PoSharedApplicationInsights'
  scope: resourceGroup(sharedResourceGroupName)
}

// App Service for hosting the .NET application
module appService 'br/public:avm/res/web/site:0.3.4' = {
  name: 'app-service'
  params: {
    name: 'PoCoupleQuiz'
    location: location
    tags: union(tags, { 'azd-service-name': 'pocouplequiz-server' })
    serverFarmResourceId: existingAppServicePlan.id
    kind: 'app'
    
    // Configure for .NET 9.0 on Windows
    siteConfig: {
      windowsFxVersion: 'DOTNET|9'
      alwaysOn: false  // Required for Free (F1) tier
      use32BitWorkerProcess: true  // Required for Free (F1) tier
      netFrameworkVersion: 'v9.0'
      httpLoggingEnabled: true
      logsDirectorySizeLimit: 35
      requestTracingEnabled: true
    }

    // Application settings including shared resource connections
    appSettingsKeyValuePairs: {
      APPLICATIONINSIGHTS_CONNECTION_STRING: existingAppInsights.properties.ConnectionString
      APPINSIGHTS_INSTRUMENTATIONKEY: existingAppInsights.properties.InstrumentationKey
      ApplicationInsightsAgent_EXTENSION_VERSION: '~3'
      XDT_MicodeAgent_MODE: 'recommended'
      ASPNETCORE_ENVIRONMENT: 'Production'
      
      // Azure Storage connection (will be updated with actual values via Azure App Service Configuration)
      AzureStorage__ConnectionString: 'DefaultEndpointsProtocol=https;EndpointSuffix=${environment().suffixes.storage};AccountName=posharedtablestorage;AccountKey=PLACEHOLDER'
      
      // Azure OpenAI connection (will be updated with actual values via Azure App Service Configuration)
      AzureOpenAI__Endpoint: 'https://posharedopenaieastus.openai.azure.com/'
      AzureOpenAI__Key: 'PLACEHOLDER'
      AzureOpenAI__DeploymentName: 'gpt-35-turbo'
      
      // Game settings
      GameSettings__KingAnswerTimeSeconds: '45'
      GameSettings__PlayerAnswerTimeEasySeconds: '40'
      GameSettings__PlayerAnswerTimeMediumSeconds: '30'
      GameSettings__PlayerAnswerTimeHardSeconds: '20'
    }
  }
}

output AZURE_APP_SERVICE_NAME string = appService.outputs.name
output AZURE_APP_SERVICE_URI string = 'https://${appService.outputs.defaultHostname}'
output AZURE_RESOURCE_POCOUPLEQUIZ_SERVER_ID string = appService.outputs.resourceId
