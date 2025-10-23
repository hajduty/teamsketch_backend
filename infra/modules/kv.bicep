/* param location string = 'centralindia'
param keyVaultName string = 'ts-keyvault-3299'
param kubeletIdentityObjectId string

@description('Secrets passed at deployment, never stored in repo')
@secure()
param saPassword string
@secure()
param sqlConnection string

resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableSoftDelete: true
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: kubeletIdentityObjectId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
  }
}

resource saPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: kv
  name: 'sa-password'
  properties: { value: saPassword }
}

resource sqlConn 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: kv
  name: 'sql-connection'
  properties: { value: sqlConnection }
}
 */
