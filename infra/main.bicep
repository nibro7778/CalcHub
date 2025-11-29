targetScope = 'resourceGroup'

@minLength(1)
@maxLength(64)
@description('Name of the environment used for resource naming')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string = ''

// Generate unique resource token
var resourceToken = uniqueString(subscription().id, resourceGroup().id, location, environmentName)

// Resource name prefixes
var appServicePlanName = 'plan-${environmentName}'
var apiAppName = 'azapi${resourceToken}'
var frontendAppName = 'azweb${resourceToken}'

// Tags
var tags = {
  'azd-env-name': environmentName
}

// User-assigned managed identity (required by AZD)
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-${environmentName}'
  location: location
  tags: tags
}

// Log Analytics Workspace
module logAnalytics './core/monitoring/log-analytics.bicep' = {
  name: 'log-analytics'
  params: {
    name: 'azlog${resourceToken}'
    location: location
    tags: tags
  }
}

// Application Insights
module applicationInsights './core/monitoring/app-insights.bicep' = {
  name: 'app-insights'
  params: {
    name: 'azai${resourceToken}'
    location: location
    tags: tags
    logAnalyticsWorkspaceId: logAnalytics.outputs.id
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    family: 'B'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// Backend API App Service
module apiApp './app/api.bicep' = {
  name: 'api-app-service'
  params: {
    name: apiAppName
    location: location
    tags: union(tags, {
      'azd-service-name': 'api'
    })
    appServicePlanId: appServicePlan.id
    managedIdentityId: managedIdentity.id
    applicationInsightsConnectionString: applicationInsights.outputs.connectionString
    frontendUrl: 'https://${frontendAppName}.azurewebsites.net'
  }
}

// Frontend App Service
module frontendApp './app/frontend.bicep' = {
  name: 'frontend-app-service'
  params: {
    name: frontendAppName
    location: location
    tags: union(tags, {
      'azd-service-name': 'frontend'
    })
    appServicePlanId: appServicePlan.id
    managedIdentityId: managedIdentity.id
    applicationInsightsConnectionString: applicationInsights.outputs.connectionString
    apiUrl: apiApp.outputs.uri
  }
}

// Required outputs
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output RESOURCE_GROUP_ID string = resourceGroup().id

// Service outputs
output API_URI string = apiApp.outputs.uri
output API_NAME string = apiApp.outputs.name
output FRONTEND_URI string = frontendApp.outputs.uri
output FRONTEND_NAME string = frontendApp.outputs.name

// Monitoring outputs
output APPLICATIONINSIGHTS_CONNECTION_STRING string = applicationInsights.outputs.connectionString
output APPLICATIONINSIGHTS_NAME string = applicationInsights.outputs.name
