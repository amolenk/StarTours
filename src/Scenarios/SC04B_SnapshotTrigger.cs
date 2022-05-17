using StarTours.Shared;
using StarTours.Shared.EventStore;
using StarTours.Domain;

namespace StarTours.Scenarios;

public class SC04B_SnapshotTrigger
{
    private readonly CosmosEventStore _eventStore;

    public SC04B_SnapshotTrigger()
    {
        _eventStore = new CosmosEventStore(
            "StarTours.Domain.Events.{0}, StarTours",
            Config.CosmosDb.Sql.Endpoint,
            Config.CosmosDb.Sql.AuthorizationKey,
            Config.CosmosDb.Sql.DatabaseId);
    }

    public async Task RunAsync()
    {
        var flight = new Flight("815", "tatooine", "hoth");
        flight.AssignRoute(new[] { new FlightLeg("tatooine", "hoth", "SH-19")});

        for (var i = 0; i < 3; i++)
        {
            flight.Divert(new[] { new FlightLeg("tatooine", "dagobah", "SH-99")});
            flight.Divert(new[] { new FlightLeg("tatooine", "hoth", "SH-19")});
        }

        flight.Cancel();

        var result = await _eventStore.AppendToStreamAsync(
            $"flight:{flight.FlightNumber}",
            flight.Version,
            flight.PendingChanges);

        if (result)
        {
            Console.WriteLine($"{flight.PendingChanges.Count} events written to stream.");
        }
        else
        {
            Console.WriteLine("Failed to write events.");
        }
    }
}
