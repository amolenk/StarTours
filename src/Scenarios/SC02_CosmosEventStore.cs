using StarTours.Domain;
using StarTours.Shared;
using StarTours.Shared.EventStore;

namespace StarTours.Scenarios;

public class SC02_CosmosEventStore
{
    public async Task RunAsync()
    {
        var eventStore = new CosmosEventStore(
            "StarTours.Domain.Events.{0}, StarTours",
            Config.CosmosDb.Sql.Endpoint,
            Config.CosmosDb.Sql.AuthorizationKey,
            Config.CosmosDb.Sql.DatabaseId);

        // Schedule a new flight.
        var flight = new Flight("1401", "thx1138", "coruscant");

        // Assign a route.
        flight.AssignRoute(new[] { new FlightLeg("thx1138", "coruscant", "SP-38") });

        // Store events in Cosmos DB
        var streamId = $"flight:{flight.FlightNumber}";
        //
        await eventStore.AppendToStreamAsync(
            streamId,
            flight.Version,
            flight.PendingChanges);

        Console.WriteLine($"Written {flight.PendingChanges.Count} events to event store.");

        Console.WriteLine("Press any key to rehydrate from event store...");
        Console.ReadKey();
        Console.WriteLine();

        // Rehydrate aggregate from events.
        var events = await eventStore.LoadStreamAsync(streamId);
        //
        var replayedFlight = new Flight(events);

        Console.WriteLine("CURRENT FLIGHT STATUS:");
        Console.WriteLine($"Version     = {replayedFlight.Version}");
        Console.WriteLine($"Origin      = {replayedFlight.OriginId}");
        Console.WriteLine($"Destination = {replayedFlight.DestinationId}");
        Console.WriteLine($"Ship        = {replayedFlight.Route.First().ShipId}");
    }
}
