param location string
param acrName string

resource acr 'Microsoft.ContainerRegistry/registries@2025-04-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {}
}

output acrId string = acr.id
