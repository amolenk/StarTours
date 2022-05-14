# Create a SQL API database and container

# Variable block
location="West Europe"
resourceGroup="StarToursDemos"
account="cosmos-startours-sql"
database="cosmos-startours-sql-db"
container="streams"
partitionKey="/streamId"

# Create a resource group
echo "Creating $resourceGroup in $location..."
az group create \
    --name $resourceGroup \
    --location "$location"

# Create a Cosmos account for SQL API
echo "Creating $account"
az cosmosdb create \
    --name $account \
    --resource-group $resourceGroup \
    --kind GlobalDocumentDB \
    --locations regionName="$location" failoverPriority=0 \
    --default-consistency-level "Session" \
    --enable-free-tier true

# Create a SQL API database
echo "Creating $database"
az cosmosdb sql database create \
    --account-name $account \
    --resource-group $resourceGroup \
    --name $database

# Create a SQL API container
echo "Creating $container"
az cosmosdb sql container create \
    --account-name $account \
    --resource-group $resourceGroup \
    --database-name $database \
    --name $container \
    --partition-key-path $partitionKey \
    --throughput 400

# Printing connection string
az cosmosdb keys list \
    --name $account \
    --resource-group $resourceGroup \
    --type connection-strings
