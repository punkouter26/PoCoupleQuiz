targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

// Tags that should be applied to all resources.
var tags = {
  'azd-env-name': environmentName
}

// Generate resource group name from solution name
var resourceGroupName = 'PoCoupleQuiz'

// Organize resources in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

module resources 'resources.bicep' = {
  scope: rg
  name: 'resources'
  params: {
    location: location
    tags: tags
    serviceName: 'web'
  }
}

// ============================================================================
// OUTPUTS FOR AZURE DEPLOYMENT
// ============================================================================
// These outputs are used by azd for deployment and local development configuration.
// Run 'azd env get-values' to see these values.
// ============================================================================

// Azure subscription and resource group
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_RESOURCE_GROUP string = rg.name

// Application Insights (for app telemetry)
output APPLICATIONINSIGHTS_CONNECTION_STRING string = resources.outputs.APPLICATION_INSIGHTS_CONNECTION_STRING
output APPLICATIONINSIGHTS_INSTRUMENTATION_KEY string = resources.outputs.APPLICATION_INSIGHTS_INSTRUMENTATION_KEY

// Log Analytics (for monitoring)
output LOG_ANALYTICS_WORKSPACE_ID string = resources.outputs.LOG_ANALYTICS_WORKSPACE_ID

// Azure OpenAI (shared resource)
output AZURE_OPENAI_ENDPOINT string = resources.outputs.OPENAI_ENDPOINT
output AZURE_OPENAI_DEPLOYMENT_NAME string = resources.outputs.OPENAI_DEPLOYMENT_NAME

// Azure Storage Account
output AZURE_STORAGE_ACCOUNT_NAME string = resources.outputs.STORAGE_ACCOUNT_NAME

// App Service (web application)
output SERVICE_WEB_NAME string = resources.outputs.SERVICE_WEB_NAME
output SERVICE_WEB_URI string = resources.outputs.SERVICE_WEB_URI
output APP_SERVICE_URL string = resources.outputs.APP_SERVICE_URL
