using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace EventStore;

public class CosmosEventStore : IEventStore
{
    private readonly string _eventTypeFormat;
    private readonly CosmosClient _client;
    private readonly string _databaseId;
    private readonly string _containerId;

    public CosmosEventStore(
        string eventTypeFormat,
        string connectionString, 
        string databaseId,
        string containerId = "events")
    {
        _eventTypeFormat = eventTypeFormat;
        _client = new CosmosClient(connectionString);
        _databaseId = databaseId;
        _containerId = containerId;
    }

    public async Task<IList<IEvent>> LoadStreamAsync(string streamId)
    {
        Container container = _client.GetContainer(_databaseId, _containerId);

        var sqlQueryText = "SELECT * FROM events e"
            + " WHERE e.id <> 'version' AND e.stream = @streamId"
            + " ORDER BY e.version";

        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText)
            .WithParameter("@streamId", streamId);

        var events = new List<IEvent>();

        FeedIterator<EventDocument> feedIterator = container.GetItemQueryIterator<EventDocument>(queryDefinition);
        while (feedIterator.HasMoreResults)
        {
            FeedResponse<EventDocument> response = await feedIterator.ReadNextAsync();
            foreach (var eventDocument in response)
            {
                events.Add(eventDocument.GetEvent(_eventTypeFormat));
            }
        }

        return events;
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
        var eventEnvelopes = events.Select(e => EventDocument.Create(streamId, ++expectedVersion, e));

        return JsonConvert.SerializeObject(eventEnvelopes);
    }
}
