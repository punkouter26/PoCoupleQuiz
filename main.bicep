@description('The name of the project/solution')
param projectName string = 'PoCoupleQuiz'

@description('The location for all resources')
param location string = resourceGroup().location

// Reference existing storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: 'pocouplequizstorage'
  scope: resourceGroup('PoCoupleQuiz')
}

// Reference existing App Service Plan from PoShared resource group
resource sharedAppServicePlan 'Microsoft.Web/serverfarms@2023-01-01' existing = {
  name: 'PoSharedFree'
  scope: resourceGroup('PoShared')
}

// Web App using shared app service plan
resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: projectName
  location: location
  kind: 'app'
  properties: {
    serverFarmId: sharedAppServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'AzureStorage__ConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'AzureOpenAI__Endpoint'
          value: 'https://poshared.openai.azure.com/'
        }
        {
          name: 'AzureOpenAI__Key'
          value: '6FlBrnPuXn2gUjyqVWiFFqu0Ma7I57TUavukI9ZixcaDFmNDhFVdJQQJ99BAACYeBjFXJ3w3AAABACOGIyFf'
        }
        {
          name: 'AzureOpenAI__DeploymentName'
          value: 'gpt-35-turbo'
        }
      ]
    }
  }
}

// Output the web app URL
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output storageAccountName string = storageAccount.name
