using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Diagnostics;

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

        var sqlQueryText = "SELECT e.type, e.data FROM events e"
            + " WHERE e.id <> 'version' AND e.stream = @streamId"
            + " ORDER BY e.version";

        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText)
            .WithParameter("@streamId", streamId);

        var events = new List<IEvent>();

        var sw = Stopwatch.StartNew();
        double ru = -1;

        FeedIterator<EventDocument> feedIterator = container.GetItemQueryIterator<EventDocument>(
            queryDefinition,
            requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(streamId) });

        while (feedIterator.HasMoreResults)
        {
            FeedResponse<EventDocument> feedResponse = await feedIterator.ReadNextAsync();
            ru = feedResponse.RequestCharge;

            foreach (var eventDocument in feedResponse)
            {
                //events.Add(eventDocument.GetEvent(_eventTypeFormat));
            }
        }

        var elapsted = sw.ElapsedMilliseconds;

        Console.WriteLine(ru.ToString());
        Console.WriteLine(elapsted);
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
