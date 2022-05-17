using StarTours.Shared;
using StarTours.Shared.EventStore;
using StarTours.Domain;

namespace StarTours.Scenarios;

public class SC04C_SnapshotLoad
{
    private readonly CosmosEventStore _eventStore;

    public SC04C_SnapshotLoad()
    {
        _eventStore = new CosmosEventStore(
            "StarTours.Domain.Events.{0}, StarTours",
            Config.CosmosDb.Sql.Endpoint,
            Config.CosmosDb.Sql.AuthorizationKey,
            Config.CosmosDb.Sql.DatabaseId);
    }

    public async Task RunAsync()
    {
        var streamId = "flight:815";

        var (snapshot, snapshotVersion) = await _eventStore.LoadSnapshotAsync<Flight.Snapshot>(
            streamId);

        Console.WriteLine($"Found snapshot for version {snapshotVersion}.");

        var events = await _eventStore.LoadStreamAsync(
            streamId,
            minVersion: snapshotVersion + 1);

        Console.WriteLine($"Found {events.Count} additional events.");

        var flight = new Flight(snapshot, snapshotVersion, events);

        Console.WriteLine("CURRENT FLIGHT STATUS:");
        Console.WriteLine($"Version     = {flight.Version}");
        Console.WriteLine($"Origin      = {flight.OriginId}");
        Console.WriteLine($"Destination = {flight.DestinationId}");
        Console.WriteLine($"Canceled?   = {flight.IsCanceled}");
    }
}
