targetScope = 'resourceGroup'

param environmentName string = 'dev'

param location string = resourceGroup().location

param imageTag string

param sqlLocation string = location

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
  location: sqlLocation
  properties: {
    administratorLogin: administratorSqlLogin
    administratorLoginPassword: administratorLoginPassword
  }
}

resource database 'Microsoft.Sql/servers/databases@2025-01-01' = {
  parent: sqlServer
  name: 'db-fooddelivery'
  location: sqlLocation
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

resource insights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'insights-${environmentName}-${uniqueSuffix}'
  kind: 'web'
  location: location
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2026-01-01' = {
  name: 'env-fooddelivery-${environmentName}-${uniqueSuffix}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2026-01-01' = {
  name: 'asb-fooddelivery-${environmentName}-${uniqueSuffix}'
  location: location
  sku: {
    tier: 'Standard'
    name: 'Standard'
  }
}

resource serviceBusAuthRule 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2023-01-01-preview' = {
  parent: serviceBus
  name: 'app-access'
  properties: {
    rights: [
      'Send'
      'Listen'
      'Manage'
    ]
  }
}

resource kvStore 'Microsoft.KeyVault/vaults@2026-02-01' = {
  name: take('kv-fdel-${environmentName}-${uniqueSuffix}', 24)
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenant().tenantId
    enableRbacAuthorization: true
  }
}

resource sqlConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2026-02-01' = {
  parent: kvStore
  name: 'sql-connection-string'
  properties: {
    value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${database.name};User ID=${administratorSqlLogin};Password=${administratorLoginPassword};Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;'
  }
}

resource serviceBusConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2026-02-01' = {
  parent: kvStore
  name: 'service-bus-connection-string'
  properties: {
    value: serviceBusAuthRule.listKeys().primaryConnectionString
  }
}

resource containerApp 'Microsoft.App/containerApps@2026-01-01' = {
  name: 'api-fooddelivery-${environmentName}'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8000
      }
      registries: [{
        server: containerRegistry.properties.loginServer
        identity: 'system'
      }]
      secrets: [
        {
          name: 'sql-connection-string'
          keyVaultUrl: '${kvStore.properties.vaultUri}secrets/sql-connection-string'
          identity: 'system'
        }
        {
          name: 'service-bus-connection-string'
          keyVaultUrl: '${kvStore.properties.vaultUri}secrets/service-bus-connection-string'
          identity: 'system'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: image: 'mcr.microsoft.com/k8se/quickstart:latest'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'sql-connection-string'
            }
            {
              name: 'ConnectionStrings__AzureServiceBus'
              secretRef: 'service-bus-connection-string'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: insights.properties.ConnectionString
            }
          ]
        }
      ]
    }
  }
}

resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(containerRegistry.id, containerApp.id, 'AcrPull')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: containerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource kvPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: kvStore
  name: guid(kvStore.id, containerApp.id, 'KeyVaultSecretsUser')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
    principalId: containerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
