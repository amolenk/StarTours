using Demo.Domain;
using Demo.Shared;
using EventStore;

namespace Demo.Scenarios;

public class SC003A_SeedData
{
    private readonly CosmosEventStore _eventStore;

    public SC003A_SeedData()
    {
        _eventStore = new CosmosEventStore(
            "Demo.Domain.Events.{0}, Demo",
            Config.CosmosDb.ConnectionString,
            Config.CosmosDb.DatabaseId);
    }

    public async Task RunAsync()
    {
        var flight1 = new Flight("SC003-1", "origin", "dest");
        flight1.AssignRoute(new[] { new FlightLeg("origin", "dest", "ship") });
        flight1.Cancel();

        var flight2 = new Flight("SC003-2", "origin", "dest");
        flight2.AssignRoute(new[] { new FlightLeg("origin", "dest", "ship") });

        var flight3 = new Flight("SC003-3", "origin", "dest");
        flight3.AssignRoute(new[] { new FlightLeg("origin", "dest", "ship") });
        flight3.Divert(new[] { new FlightLeg("origin", "dest", "ship") });

        await SaveFlightAsync(flight1);
        await SaveFlightAsync(flight2);
        await SaveFlightAsync(flight3);
    }

    private Task SaveFlightAsync(Flight flight) =>
        _eventStore.AppendToStreamAsync(
            $"flight:{flight.FlightNumber}",
            flight.Version,
            flight.PendingChanges);
}
