param aksName string
param rgName string

resource aks 'Microsoft.ContainerService/managedClusters@2024-02-01' existing = {
  name: aksName
  scope: resourceGroup(rgName)
}

output kubeletIdentityObjectId string = aks.properties.identityProfile.kubeletidentity.objectId
