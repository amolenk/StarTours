using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;

namespace StarTours.Shared.EventStore;

public class CosmosEventStore
{
    private readonly string _eventTypeFormat;
    private readonly CosmosClient _client;
    private readonly string _databaseId;
    private readonly string _containerId;

    private readonly Container _container;

    public CosmosEventStore(
        string eventTypeFormat,
        string connectionString, 
        string databaseId,
        string containerId = "streams")
    {
        _eventTypeFormat = eventTypeFormat;
        _client = new CosmosClient(connectionString);
        _databaseId = databaseId;
        _containerId = containerId;

        _container = _client.GetContainer(databaseId, containerId); 
    }

    public async Task<bool> AppendToStreamAsync(
        string streamId,
        int expectedVersion,
        IEnumerable<IEvent> events)
    {
        Container container = _client.GetContainer(_databaseId, _containerId);

        return await container.Scripts.ExecuteStoredProcedureAsync<bool>(
            "spAppendToStream",
            new PartitionKey(streamId),
            new dynamic[]
            {
                streamId,
                expectedVersion,
                SerializeEvents(streamId, expectedVersion, events)
            });
    }

    public async Task<IList<IEvent>> LoadStreamAsync(
        string streamId,
        int minVersion = int.MinValue,
        int maxVersion = int.MaxValue)
    {
        Container container = _client.GetContainer(_databaseId, _containerId);

        var sqlQueryText = "SELECT e.type, e.data FROM events e"
            + " WHERE e.streamId = @streamId"
                + " AND e.version >= @minVersion"
                + " AND e.version <= @maxVersion"
                + " AND e.id <> 'version'"
            + " ORDER BY e.version";

        var queryDefinition = new QueryDefinition(sqlQueryText)
            .WithParameter("@streamId", streamId)
            .WithParameter("@minVersion", minVersion)
            .WithParameter("@maxVersion", maxVersion);

        var events = new List<IEvent>();

        var feedIterator = container.GetItemQueryIterator<EventDocument>(
            queryDefinition,
            requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(streamId) });

        while (feedIterator.HasMoreResults)
        {
            var feedResponse = await feedIterator.ReadNextAsync();

            foreach (var eventDocument in feedResponse)
            {
                events.Add(eventDocument.Deserialize(_eventTypeFormat));
            }
        }

        return events;
    }

    private static string SerializeEvents(string streamId, int expectedVersion, IEnumerable<IEvent> events)
    {
        var eventEnvelopes = events.Select(e => EventDocument.Create(streamId, ++expectedVersion, e));

        return JsonConvert.SerializeObject(eventEnvelopes);
    }

    #region Snapshots

    public async Task SaveSnapshotAsync(string streamId, int version, object snapshot)
    {
        var response = await _container.UpsertItemAsync(
            SnapshotDocument.Create(streamId, version, snapshot),
            new PartitionKey(streamId));

        if (response.StatusCode != HttpStatusCode.Created)
        {
            throw new Exception("Failed to save snapshot. Got result: " + response.StatusCode);
        }
    }

    public async Task<(T Snapshot, int Version)> LoadSnapshotAsync<T>(string streamId)
    {
        var response = await _container.ReadItemAsync<SnapshotDocument>(
            "snapshot",
            new PartitionKey(streamId));

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            return (
                response.Resource.Deserialize<T>(),
                response.Resource.Version);
        }

        return (default(T), 0);
    }

    #endregion
}
