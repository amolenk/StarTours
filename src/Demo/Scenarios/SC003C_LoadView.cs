using Demo.Shared;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Demo.Scenarios;

public class SC003C_LoadView
{
    public async Task RunAsync()
    {
        var client = new CosmosClient(Config.CosmosDb.ConnectionString);
        var container = client.GetContainer(Config.CosmosDb.DatabaseId, Config.CosmosDb.Containers.Views);

        var sqlQueryText = "SELECT * FROM views v";

        var feedIterator = container.GetItemQueryIterator<FlightStatusDocument>(
            sqlQueryText,
            requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey("flightStatus") });

        while (feedIterator.HasMoreResults)
        {
            var feedResponse = await feedIterator.ReadNextAsync();

            foreach (var document in feedResponse)
            {
                Console.WriteLine($"{document.FlightNumber} = {document.FlightStatus}");
            }
        }
    }

    public class FlightStatusDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string FlightNumber { get; set; }
            
        [JsonProperty(PropertyName = "status")]
        public string FlightStatus { get; set; }

        [JsonProperty(PropertyName = "version")]
        public int Version { get; set; }
    }
}
