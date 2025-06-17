@description('The name of the application')
param applicationName string = 'pocouplequiz'

@description('The location for all resources')
param location string = resourceGroup().location

@description('The SKU of the App Service Plan')
param appServicePlanSku string = 'B1'

@description('The SKU of the Storage Account')
param storageAccountSku string = 'Standard_LRS'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: '${applicationName}-plan'
  location: location
  sku: {
    name: appServicePlanSku
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: applicationName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      appSettings: [
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
  }
}

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: '${applicationName}storage'
  location: location
  sku: {
    name: storageAccountSku
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

// Output the storage account connection string
output storageConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}' 
