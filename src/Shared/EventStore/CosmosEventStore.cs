using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Net;

namespace StarTours.Shared.EventStore;

public class CosmosEventStore
{
    private readonly string _eventTypeFormat;
    private readonly CosmosClient _client;
    private readonly Container _container;

    public CosmosEventStore(
        string eventTypeFormat,
        string endpoint,
        string authorizationKey, 
        string databaseId,
        string containerId = "streams")
    {
        _eventTypeFormat = eventTypeFormat;
        _client = new CosmosClient(endpoint, authorizationKey);
        _container = _client.GetContainer(databaseId, containerId); 
    }

    public async Task<bool> AppendToStreamAsync(
        string streamId,
        int expectedVersion,
        IEnumerable<IEvent> events)
    {
        var partitionKey = new PartitionKey(streamId);
        var batch = _container.CreateTransactionalBatch(partitionKey);

        var newVersion = expectedVersion + events.Count();

        if (expectedVersion == 0)
        {
            batch.CreateItem(VersionDocument.Create(streamId, newVersion));
        }
        else
        {
            ItemResponse<VersionDocument> versionDocumentResponse;

            try
            {
                versionDocumentResponse = await _container.ReadItemAsync<VersionDocument>(
                    "version", partitionKey);

                if (versionDocumentResponse.Resource.Version != expectedVersion)
                {
                    // Version in the database is different.
                    return false;
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Version in the database does not exist, so cannot match.
                return false;
            }

            batch.ReplaceItem(
                "version",
                VersionDocument.Create(streamId, newVersion),
                new TransactionalBatchItemRequestOptions
                {
                    IfMatchEtag = versionDocumentResponse.ETag
                });
        }

        foreach (var @event in events)
        {
            batch.CreateItem(
                EventDocument.Create(streamId, ++expectedVersion, @event));
        }

        var batchResult = await batch.ExecuteAsync();

        if (batchResult.IsSuccessStatusCode)
        {
            return true;
        }
        else if (batchResult.StatusCode == HttpStatusCode.Conflict)
        {
            return false;
        }
        else
        {
            throw new Exception("Got status code: " + batchResult.StatusCode);
        }
    }

    public async Task<IList<IEvent>> LoadStreamAsync(
        string streamId,
        int minVersion = int.MinValue,
        int maxVersion = int.MaxValue)
    {
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

        var feedIterator = _container.GetItemQueryIterator<EventDocument>(
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
