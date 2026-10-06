param location string = resourceGroup().location
param aksName string
param acrName string
param rgName string
param keyVaultName string

param mysqlServerName string = 'teamsketch-mysql'
param mysqlAdminUsername string = 'teamsketch'

@secure()
param mysqlPassword string

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

module mysql 'mysql.bicep' = {
  name: 'mysqlModule'
  params: {
    location: location
    serverName: mysqlServerName
    adminUsername: mysqlAdminUsername
    adminPassword: mysqlPassword
  }
}

/* module kvModule 'kv.bicep' = {
  name: 'kvDeployment'
  scope: resourceGroup(rgName)
  params: {
    location: location
    kubeletIdentityObjectId: aksCsi.outputs.kubeletIdentityObjectId
    keyVaultName: keyVaultName
    saPassword: mysqlPassword
    sqlConnection: 'Server=${mysql.outputs.fqdn};Port=3306;Database=${mysql.outputs.databaseName};User ID=${mysqlAdminUsername};Password=${mysqlPassword};SslMode=Required;'
  }
}
 */

output mysqlFqdn string = mysql.outputs.fqdn
