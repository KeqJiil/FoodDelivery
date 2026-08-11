targetScope = 'resourceGroup'

param environmentName string = 'dev'

param location string = resourceGroup().location

@secure()
param administratorSqlLogin string

@secure()
param administratorLoginPassword string

var uniqueSuffix = uniqueString(resourceGroup().id)

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2025-07-01' = {
  name: 'log-fooddelivery-${environmentName}-${uniqueSuffix}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-11-01' = {
  name: 'acrfooddelivery${environmentName}${uniqueSuffix}'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    anonymousPullEnabled: false
  }
}

resource sqlServer 'Microsoft.Sql/servers@2025-01-01' = {
  name: 'sqlfooddelivery-${environmentName}-${uniqueSuffix}'
  location: location
  properties: {
    administratorLogin: administratorSqlLogin
    administratorLoginPassword: administratorLoginPassword
  }
}

resource database 'Microsoft.Sql/servers/databases@2025-01-01' = {
  parent: sqlServer
  name: 'db-fooddelivery'
  location: location
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 2
  }
  properties: {
    autoPauseDelay: 15
    minCapacity: json('0.5')
    maxSizeBytes: 2147483648
  }
}

resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  parent: sqlServer
  name: 'AllowAllAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   scope: containerRegistry
//   name: guid(containerRegistry.id, containerApp.id, 'AcrPull')
//   properties: {
//     roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
//     principalId: containerApp.identity.proncipalId
//     principalType: 'ServicePrincipal'
//   }
// }

