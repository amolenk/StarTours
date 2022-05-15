# Create a Gremlin database and graph

# Variable block
location="West Europe"
resourceGroup="StarToursDemos"
account="cosmos-startours-gremlin"
database="cosmos-startours-gremlin-db"
graph="flights"

# Create a resource group
echo "Creating $resourceGroup in $location..."
az group create \
    --name $resourceGroup \
    --location "$location"

# Create a Cosmos account for Gremlin API
echo "Creating $account"
az cosmosdb create \
    --name $account \
    --resource-group $resourceGroup \
    --capabilities EnableGremlin EnableServerless \
    --default-consistency-level Eventual \
    --locations regionName="$location" failoverPriority=0 \
    --default-consistency-level "Session"

# Create a Gremlin database
echo "Creating $database with $account"
az cosmosdb gremlin database create \
    --account-name $account \
    --resource-group $resourceGroup \
    --name $database

# Create a Gremlin graph
echo "Creating $graph"
az cosmosdb gremlin graph create \
    --account-name $account \
    --resource-group $resourceGroup \
    --database-name $database \
    --name $graph \
    --partition-key-path "/partitionKey"
