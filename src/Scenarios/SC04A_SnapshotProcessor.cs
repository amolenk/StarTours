using StarTours.Shared;
using StarTours.Shared.EventStore;
using Microsoft.Azure.Cosmos;
using StarTours.Domain;

namespace StarTours.Scenarios;

public class SC04A_SnapshotProcessor
{
    private readonly CosmosEventStore _eventStore;

    public SC04A_SnapshotProcessor()
    {
        _eventStore = new CosmosEventStore(
            "StarTours.Domain.Events.{0}, StarTours",
            Config.CosmosDb.Sql.Endpoint,
            Config.CosmosDb.Sql.AuthorizationKey,
            Config.CosmosDb.Sql.DatabaseId);
    }

    public async Task RunAsync()
    {
        var cosmosClient = new CosmosClient(Config.CosmosDb.Sql.Endpoint, Config.CosmosDb.Sql.AuthorizationKey);

        var monitorContainer = cosmosClient.GetContainer(
            Config.CosmosDb.Sql.DatabaseId,
            Config.CosmosDb.Sql.Containers.Streams);

        var leaseContainer = cosmosClient.GetContainer(
            Config.CosmosDb.Sql.DatabaseId,
            Config.CosmosDb.Sql.Containers.Leases);

        var processor = monitorContainer
            .GetChangeFeedProcessorBuilder<EventDocument>("SnapshotProcessor", HandleChangesAsync)
            .WithInstanceName("local-worker")
            .WithLeaseContainer(leaseContainer)
            .WithStartTime(DateTime.MinValue.ToUniversalTime())
            .WithErrorNotification(HandleErrorAsync)
            .WithLeaseAcquireNotification(HandleLeaseAcquiredAsync)
            .WithLeaseReleaseNotification(HandleLeaseReleasedAsync)
            .Build();

        await processor.StartAsync();

        Console.ReadKey();

        await processor.StopAsync();
    }

    private async Task HandleChangesAsync(
        IReadOnlyCollection<EventDocument> events,
        CancellationToken cancellationToken)
    {
        var flightEvents = events.Where(doc =>
            doc.StreamId.StartsWith("flight:") &&
            doc.Id.StartsWith("event:") &&
            doc.Version % 10 == 0);

        foreach (var eventDocument in flightEvents)
        {
            Console.WriteLine($"{eventDocument.StreamId}/{eventDocument.Id}");

            var streamEvents = await _eventStore.LoadStreamAsync(
                eventDocument.StreamId,
                maxVersion: eventDocument.Version);
            
            var flight = new Flight(streamEvents);

            var snapshot = flight.CreateSnapshot();

            await _eventStore.SaveSnapshotAsync(
                eventDocument.StreamId,
                eventDocument.Version,
                snapshot);
        }
    }

    private Task HandleErrorAsync(string leaseToken, Exception exception)
    {
        Console.WriteLine(exception);
        return Task.CompletedTask;
    }

    private Task HandleLeaseAcquiredAsync(string leaseToken)
    {
        Console.WriteLine($"Lease {leaseToken} is acquired and will start processing");
        return Task.CompletedTask;
    }

    private Task HandleLeaseReleasedAsync(string leaseToken)
    {
        Console.WriteLine($"Lease {leaseToken} is released and processing is stopped");
        return Task.CompletedTask;
    }
}
