# Create a Gremlin database and graph

# Variable block
location="West Europe"
resourceGroup="StarToursDemos"
account="cosmos-startours-gremlin"
database="cosmos-startours-gremlin-db"
graph="flights"
#partitionKey="/stream"

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
    --capabilities EnableGremlin \
    --default-consistency-level Eventual \
    --locations regionName="$location" failoverPriority=0 \
    --default-consistency-level "Session"

# Create a Gremlin database
echo "Creating $database with $account"
az cosmosdb gremlin database create \
    --account-name $account \
    --resource-group $resourceGroup \
    --name $database

# # Define the index policy for the graph, include spatial and composite indexes
# printf ' 
# {
#     "indexingMode": "consistent", 
#     "includedPaths": [
#         {"path": "/*"}
#     ],
#     "excludedPaths": [
#         { "path": "/headquarters/employees/?"}
#     ],
#     "spatialIndexes": [
#         {"path": "/*", "types": ["Point"]}
#     ],
#     "compositeIndexes":[
#         [
#             { "path":"/name", "order":"ascending" },
#             { "path":"/age", "order":"descending" }
#         ]
#     ]
# }' > "idxpolicy-$randomIdentifier.json"

# Create a Gremlin graph
echo "Creating $graph"
az cosmosdb gremlin graph create \
    --account-name $account \
    --resource-group $resourceGroup \
    --database-name $database \
    --name $graph \
    --throughput 400 \
    --partition-key-path "/partitionKey"

# Clean up temporary index policy file
#rm -f "idxpolicy-$randomIdentifier.json"

# Printing connection string
az cosmosdb keys list \
    --name $account \
    --resource-group $resourceGroup \
    --type connection-strings
