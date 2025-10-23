param location string = resourceGroup().location
param aksName string
param acrName string
param rgName string
param keyVaultName string

param sqlServerName string = 'teamsketch-sql'
param sqlDbName string = 'teamsketch-db' 
param sqlAdminUsername string = 'teamsa'

@secure()
param sqlPassword string

module acr 'acr.bicep' = {
  name: 'acrModule'
  params: {
    location: location
    acrName: acrName
  }
}

module aks 'aks.bicep' = {
  name: 'aksModule'
  params: {
    location: location
    aksName: aksName
  }
}

/* module aksCsi 'aks-csi.bicep' = {
  name: 'aksCsiModule'
  params: {
    aksName: aksName
    rgName: rgName
  }
} */

module roleAssign 'roleassign.bicep' = {
  name: 'roleAssignmentModule'
  params: {
    aksPrincipalId: aks.outputs.principalId
    acrId: acr.outputs.acrId
  }
}

module sql 'sql.bicep' = {
  name: 'sqlModule'
  params: {
    location: location
    sqlServerName: sqlServerName
    sqlDbName: sqlDbName
    sqlAdminUsername: sqlAdminUsername
    sqlAdminPassword: sqlPassword
  }
}

/* module kvModule 'kv.bicep' = {
  name: 'kvDeployment'
  scope: resourceGroup(rgName)
  params: {
    location: location
    kubeletIdentityObjectId: aksCsi.outputs.kubeletIdentityObjectId
    keyVaultName: keyVaultName
    saPassword: sqlPassword
    sqlConnection: 'Server=tcp:${sql.outputs.sqlServerFqdn},1433;Database=${sql.outputs.databaseName};User ID=${sqlAdminUsername};Password=${sqlPassword};Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;'
  }
}
 */
