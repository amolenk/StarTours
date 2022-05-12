using StarTours.Domain;
using StarTours.Shared;
using StarTours.Shared.EventStore;

namespace StarTours.Scenarios;

public class SC003A_SeedData
{
    private readonly CosmosEventStore _eventStore;

    public SC003A_SeedData()
    {
        _eventStore = new CosmosEventStore(
            "StarTours.Domain.Events.{0}, StarTours",
            Config.CosmosDb.ConnectionString,
            Config.CosmosDb.DatabaseId);
    }

    public async Task RunAsync()
    {
        var flight42 = new Flight("42", "thx1138", "naboo");
        var flight45 = new Flight("45", "thx1138", "endor");
        var flight84 = new Flight("84", "thx1138", "kessel");
        var flight321 = new Flight("321", "thx1138", "bespin");
        var flight432 = new Flight("432", "thx1138", "endor");
        var flight843 = new Flight("843", "thx1138", "tatooine");
        var flight1219 = new Flight("1219", "thx1138", "hoth");
        var flight1401 = new Flight("1401", "thx1138", "coruscant");
        var flight1430 = new Flight("1430", "thx1138", "yaviniv");
        var flight2381 = new Flight("2381", "thx1138", "alderaan");

        // flight1401.AssignRoute(new[] { new FlightLeg("thx1138", "coruscant", "sp1032") });
        // flight1401.Cancel();

        // var flight2 = new Flight("SC003-2", "origin", "dest");
        // flight2.AssignRoute(new[] { new FlightLeg("origin", "dest", "ship") });

        // var flight3 = new Flight("SC003-3", "origin", "dest");
        // flight3.AssignRoute(new[] { new FlightLeg("origin", "dest", "ship") });
        // flight3.Divert(new[] { new FlightLeg("origin", "dest", "ship") });

        await SaveFlightAsync(flight42);
        await SaveFlightAsync(flight45);
        await SaveFlightAsync(flight84);
        await SaveFlightAsync(flight321);
        await SaveFlightAsync(flight432);
        await SaveFlightAsync(flight843);
        await SaveFlightAsync(flight1219);
        await SaveFlightAsync(flight1401);
        await SaveFlightAsync(flight1430);
        await SaveFlightAsync(flight2381);
    }

    private Task SaveFlightAsync(Flight flight) =>
        _eventStore.AppendToStreamAsync(
            $"flight:{flight.FlightNumber}",
            flight.Version,
            flight.PendingChanges);
}
