using StarTours.Domain.Events;
using StarTours.Shared;
using StarTours.Shared.EventStore;
using Gremlin.Net.Driver;
using StarTours.Shared.Gremlin;

namespace StarTours.Scenarios;

public class SC004B_FlightGraphProjection
{
    private const string EventTypeFormat = "StarTours.Domain.Events.{0}, StarTours";

    public async Task RunAsync()
    {
        var processor = new CosmosChangeFeedProcessor(
            Config.CosmosDb.ConnectionString,
            Config.CosmosDb.DatabaseId,
            "events",
            "leases");

        await processor.StartAsync(
            Guid.NewGuid().ToString(),
            "local",
            HandleChangesAsync);

        Console.ReadKey();

        await processor.StopAsync();
    }

    private async Task HandleChangesAsync(IReadOnlyCollection<EventDocument> events, CancellationToken cancellationToken)
    {
        var flightEvents = events.Where(doc => doc.StreamId.StartsWith("flight:") && doc.Id.StartsWith("event:"));

        foreach (var eventDocument in flightEvents)
        {
            Console.WriteLine("Got change: " + eventDocument.StreamId + "/" + eventDocument.Id);

            var actualEvent = eventDocument.GetEvent(EventTypeFormat);

            switch (actualEvent)
            {
                case FlightScheduled flightScheduled:
                    await HandleEventAsync(flightScheduled, eventDocument.Version);
                    break;

                // case FlightDiverted flightDiverted:
                //     await HandleEventAsync(flightDiverted, eventDocument.Version);
                //     break;

                // case FlightCanceled flightCanceled:
                //     await HandleEventAsync(flightCanceled, eventDocument.Version);
                //     break;
            }
        }
    }

    private async Task HandleEventAsync(FlightScheduled flightScheduled, int version)
    {
        using var gremlinClient = GremlinClientFactory.CreateClient(
            Config.CosmosDb.Gremlin.HostName,
            Config.CosmosDb.Gremlin.AuthorizationKey,
            Config.CosmosDb.Gremlin.DatabaseId,
            Config.CosmosDb.Gremlin.GraphId);

        // Upsert using Gremlin (https://stackoverflow.com/questions/52447308/add-edge-if-not-exist-using-gremlin)
        var request = $@"g.V().has('terminal','id','{flightScheduled.OriginId}').as('v')
            .V().has('terminal','id','{flightScheduled.DestinationId}')
            .coalesce(__.inE('{flightScheduled.FlightNumber}')
                .where(outV().as('v')), addE('{flightScheduled.FlightNumber}').from('v'))";

        await gremlinClient.SubmitAsync(request);
    }

    // private async Task HandleEventAsync(FlightDiverted flightDiverted, int version)
    // {
    //     FlightStatusDocument document = await _viewContainer.ReadItemAsync<FlightStatusDocument>(
    //         flightDiverted.FlightNumber,
    //         new("flightStatus"));

    //     document.FlightStatus = "Diverted";
    //     document.Version = version;

    //     await _viewContainer.UpsertItemAsync(document);
    // }

    // private async Task HandleEventAsync(FlightCanceled flightCanceled, int version)
    // {
    //     FlightStatusDocument document = await _viewContainer.ReadItemAsync<FlightStatusDocument>(
    //         flightCanceled.FlightNumber,
    //         new("flightStatus"));

    //     document.FlightStatus = "Canceled";
    //     document.Version = version;

    //     await _viewContainer.UpsertItemAsync(document);
    // }
}
