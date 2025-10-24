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
var resourceGroupName = 'rg-pocouplequiz-${environmentName}'

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
    resourceGroupName: resourceGroupName
  }
}

// Output values for the application
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_RESOURCE_GROUP string = rg.name

// App Service outputs
output AZURE_APP_SERVICE_NAME string = resources.outputs.APP_SERVICE_NAME
output AZURE_APP_SERVICE_URI string = resources.outputs.APP_SERVICE_URI

// Storage outputs
output AZURE_STORAGE_ACCOUNT_NAME string = resources.outputs.STORAGE_ACCOUNT_NAME

// Application Insights outputs
output AZURE_APPLICATION_INSIGHTS_NAME string = resources.outputs.APPLICATION_INSIGHTS_NAME
output AZURE_APPLICATION_INSIGHTS_CONNECTION_STRING string = resources.outputs.APPLICATION_INSIGHTS_CONNECTION_STRING

// Azure OpenAI outputs (using shared resource)
output AZURE_OPENAI_ENDPOINT string = resources.outputs.OPENAI_ENDPOINT
