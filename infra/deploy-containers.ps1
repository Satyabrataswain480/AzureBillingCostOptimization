param (
    [string]$resourceGroup = "MyResourceGroup",
    [string]$cosmosAccount = "MyCosmosDBAccount",
    [string]$location = "eastus"
)

# Create the resource group if it doesn't exist
if (-not (az group show --name $resourceGroup -o none 2>$null)) {
    Write-Host "Creating Resource Group..."
    az group create --name $resourceGroup --location $location | Out-Null
}

# Create Cosmos DB account
Write-Host "Creating Cosmos DB account..."
az cosmosdb create `
    --name $cosmosAccount `
    --resource-group $resourceGroup `
    --kind MongoDB `
    --locations regionName=$location failoverPriority=0 isZoneRedundant=False | Out-Null

# Create Hot container with provisioned throughput
Write-Host "Creating Hot Container..."
az cosmosdb mongodb collection create `
    --account-name $cosmosAccount `
    --database-name HotDB `
    --name HotContainer `
    --throughput 10000 | Out-Null

# Create Cold container with serverless configuration
Write-Host "Creating Cold Container..."
az cosmosdb mongodb collection create `
    --account-name $cosmosAccount `
    --database-name ColdDB `
    --name ColdContainer `
    --serverless true | Out-Null

Write-Host "Cosmos DB Hot and Cold containers created successfully."
