param location string
param aksName string

resource aks 'Microsoft.ContainerService/managedClusters@2025-07-01' = {
  name: aksName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    dnsPrefix: aksName
    agentPoolProfiles: [
      {
        name: 'nodepool1'
        vmSize: 'Standard_D2s_v6'
        count: 1
        osType: 'Linux'
        mode: 'System'
      }
    ]
    enableRBAC: true
    addonProfiles: {
      azureKeyvaultSecretsProvider: {
        enabled: true
      }
    }
  }
}

output principalId string = aks.identity.principalId
