using StarTours.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using System.Net;

namespace StarTours.Scenarios
{
    public class SC000_Migrate 
    {
        public async Task RunAsync()
        {
            CosmosClient client = new CosmosClient(Config.CosmosDb.ConnectionString);

            await client.CreateDatabaseIfNotExistsAsync(Config.CosmosDb.DatabaseId, ThroughputProperties.CreateManualThroughput(400));
            Database database = client.GetDatabase(Config.CosmosDb.DatabaseId);

            await database.DefineContainer(Config.CosmosDb.Containers.Events, "/stream").CreateIfNotExistsAsync();
            await database.DefineContainer("views", "/partitionKey").CreateIfNotExistsAsync();
            await database.DefineContainer(Config.CosmosDb.Containers.Leases, "/id").CreateIfNotExistsAsync();

            StoredProcedureProperties storedProcedureProperties = new StoredProcedureProperties
            {
                Id = "spAppendToStream",
                Body = File.ReadAllText("js/spAppendToStream.js")
            };

            Container eventsContainer = database.GetContainer("events");
            try
            {
                await eventsContainer.Scripts.DeleteStoredProcedureAsync("spAppendToStream");
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Stored procedure didn't exist yet.
            }
            await eventsContainer.Scripts.CreateStoredProcedureAsync(storedProcedureProperties);
        }
    }
}
