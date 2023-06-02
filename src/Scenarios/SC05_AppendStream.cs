using StarTours.Domain;
using StarTours.Domain.Events;
using StarTours.Shared;
using StarTours.Shared.EventStore;

namespace StarTours.Scenarios;

public class SC05_AppendStream
{
    public async Task RunAsync()
    {
        var streamId = "flight:1401";
        var stream = new List<IEvent>();

        // Schedule a new flight.
        stream.Add(new FlightScheduled
        {
            FlightNumber = "1401",
            OriginId = "thx1138",
            DestinationId = "coruscant"
        });

        // Assign a route.
        stream.Add(new FlightRouteAssigned
        {
            FlightNumber = "1401",
            Legs = new List<FlightLeg>
            {
                new FlightLeg("thx1138", "coruscant", "SP-38")
            }
        });

        // Store events in Cosmos DB
        var eventStore = new CosmosEventStore(
            "StarTours.Domain.Events.{0}, StarTours",
            Config.CosmosDb.Sql.Endpoint,
            Config.CosmosDb.Sql.AuthorizationKey,
            Config.CosmosDb.Sql.DatabaseId);
        //
        await eventStore.AppendToStreamAsync(
            streamId,
            0,
            stream);
    }
}
