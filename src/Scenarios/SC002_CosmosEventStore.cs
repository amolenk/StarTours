using StarTours.Domain;
using StarTours.Shared;
using StarTours.Shared.EventStore;

namespace StarTours.Scenarios;

public class SC002_CosmosEventStore
{
    // TODO Concurrency demo!! 2 console app instances?

    public async Task RunAsync()
    {
        var eventStore = new CosmosEventStore(
            "StarTours.Domain.Events.{0}, StarTours",
            Config.CosmosDb.ConnectionString,
            Config.CosmosDb.DatabaseId);

        // Schedule a new flight.
        var flight = new Flight("flightNr", "origin", "dest");

        // Assign a route.
        flight.AssignRoute(new[] { new FlightLeg("origin", "dest", "ship") });

        // Store events in Cosmos DB
        var streamId = $"flight:{flight.FlightNumber}";
        //
        await eventStore.AppendToStreamAsync(
            streamId,
            flight.Version,
            flight.PendingChanges);

        // Rehydrate aggregate from events.
        var events = await eventStore.LoadStreamAsync(streamId);
        //
        flight = new Flight(events);

        // Divert flight to new destination.
        flight.Divert(new[] { new FlightLeg("origin", "dest2", "ship2") });

        // Store changes in Cosmos DB
        await eventStore.AppendToStreamAsync(
            streamId,
            flight.Version,
            flight.PendingChanges);
    }
}
