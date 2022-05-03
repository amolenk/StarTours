using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventStore;

public class CosmosEventStore : IEventStore
{
    private readonly IEventTypeResolver _eventTypeResolver;
    private readonly CosmosClient _client;
    private readonly string _databaseId;
    private readonly string _containerId;

    public CosmosEventStore(
        IEventTypeResolver eventTypeResolver,
        string connectionString, 
        string databaseId,
        string containerId = "events")
    {
        _eventTypeResolver = eventTypeResolver;
        _client = new CosmosClient(connectionString);
        _databaseId = databaseId;
        _containerId = containerId;
    }

    public async Task<EventStream> LoadStreamAsync(string streamId)
    {
        Container container = _client.GetContainer(_databaseId, _containerId);

        var sqlQueryText = "SELECT * FROM events e"
            + " WHERE e.stream.id = @streamId"
            + " ORDER BY e.stream.version"; 

        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText)
            .WithParameter("@streamId", streamId);

        int version = 0;
        var events = new List<IEvent>();

        FeedIterator<EventEnvelope> feedIterator = container.GetItemQueryIterator<EventEnvelope>(queryDefinition);
        while (feedIterator.HasMoreResults)
        {
            FeedResponse<EventEnvelope> response = await feedIterator.ReadNextAsync();
            foreach (var eventWrapper in response)
            {
                //version = eventWrapper.StreamInfo.Version;

                events.Add(eventWrapper.GetEvent(_eventTypeResolver));
            }
        }

        return new EventStream(streamId, version, events);
    }

    

    public async Task<bool> AppendToStreamAsync(string streamId, int expectedVersion, IEnumerable<IEvent> events)
    {
        Container container = _client.GetContainer(_databaseId, _containerId);

        PartitionKey partitionKey = new PartitionKey(streamId);

        dynamic[] parameters = new dynamic[]
        {
            streamId,
            expectedVersion,
            SerializeEvents(streamId, expectedVersion, events)
        };

        return await container.Scripts.ExecuteStoredProcedureAsync<bool>("spAppendToStream", partitionKey, parameters);
    }

    private static string SerializeEvents(string streamId, int expectedVersion, IEnumerable<IEvent> events)
    {
        var items = events.Select(e => new EventEnvelope
        {
            Id = $"{streamId}:{++expectedVersion}",
            StreamId = streamId,
            EventType = e.GetType().Name,
            EventData = JObject.FromObject(e)
        });

        return JsonConvert.SerializeObject(items);
    }
}
