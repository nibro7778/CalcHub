param name string
param location string = resourceGroup().location
param tags object = {}
param appServicePlanId string
param managedIdentityId string
param applicationInsightsConnectionString string
param frontendUrl string

resource apiAppService 'Microsoft.Web/sites@2023-01-01' = {
  name: name
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsightsConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'CORS_ORIGINS'
          value: frontendUrl
        }
      ]
    }
  }
}

output id string = apiAppService.id
output name string = apiAppService.name
output uri string = 'https://${apiAppService.properties.defaultHostName}'
output identityPrincipalId string = apiAppService.identity.principalId
