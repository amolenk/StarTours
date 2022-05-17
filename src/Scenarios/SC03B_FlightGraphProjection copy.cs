using StarTours.Domain.Events;
using StarTours.Shared;
using StarTours.Shared.EventStore;
using Gremlin.Net.Driver;
using StarTours.Shared.Gremlin;
using Microsoft.Azure.Cosmos;

namespace StarTours.Scenarios;

public class SC03B_FlightGraphProjection
{
    private const string EventTypeFormat = "StarTours.Domain.Events.{0}, StarTours";

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
            .GetChangeFeedProcessorBuilder<EventDocument>("GraphProjection", HandleChangesAsync)
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
            doc.Id.StartsWith("event:"));

        if (flightEvents.Any())
        {
            using var gremlinClient = GremlinClientFactory.CreateClient(
                Config.CosmosDb.Gremlin.HostName,
                Config.CosmosDb.Gremlin.AuthorizationKey,
                Config.CosmosDb.Gremlin.DatabaseId,
                Config.CosmosDb.Gremlin.GraphId);

            foreach (var eventDocument in flightEvents)
            {
                Console.WriteLine($"{eventDocument.StreamId}/{eventDocument.Id}");

                var actualEvent = eventDocument.Deserialize(EventTypeFormat);

                switch (actualEvent)
                {
                    case FlightScheduled flightScheduled:
                        await HandleEventAsync(flightScheduled, gremlinClient);
                        break;

                    case FlightDiverted flightDiverted:
                        await HandleEventAsync(flightDiverted, gremlinClient);
                        break;

                    case FlightCanceled flightCanceled:
                        await HandleEventAsync(flightCanceled, gremlinClient);
                        break;
                }
            }
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

    private async Task HandleEventAsync(
        FlightScheduled flightScheduled,
        GremlinClient client)
    {
        await UpsertFlight(
            flightScheduled.FlightNumber,
            flightScheduled.OriginId,
            flightScheduled.DestinationId,
            client);
    }

    private async Task HandleEventAsync(
        FlightDiverted flightDiverted,
        GremlinClient client)
    {
        await DeleteIfExistsFlight(flightDiverted.FlightNumber, client);

        await UpsertFlight(
            flightDiverted.FlightNumber,
            flightDiverted.OriginId,
            flightDiverted.NewDestinationId,
            client);
    }

    private async Task HandleEventAsync(
        FlightCanceled flightCanceled,
        GremlinClient client)
    {
        await DeleteIfExistsFlight(flightCanceled.FlightNumber, client);
    }

    private async Task UpsertFlight(
        string flightNumber,
        string originId,
        string destinationId,
        GremlinClient client)
    {
        // Upsert using Gremlin (https://stackoverflow.com/questions/52447308/add-edge-if-not-exist-using-gremlin)
        var request = $@"g.V().has('terminal','id','{originId}').as('v')
            .V().has('terminal','id','{destinationId}')
            .coalesce(__.inE('{flightNumber}')
                .where(outV().as('v')), addE('{flightNumber}').from('v'))";

        await client.SubmitAsync(request);
    }

    private async Task DeleteIfExistsFlight(string flightNumber, GremlinClient client)
    {
        var request = $"g.E().hasLabel('{flightNumber}').drop()";
        
        await client.SubmitAsync(request);
    }
}
