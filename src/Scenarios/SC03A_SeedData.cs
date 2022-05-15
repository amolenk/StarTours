using StarTours.Domain;
using StarTours.Shared;
using StarTours.Shared.EventStore;

namespace StarTours.Scenarios;

public class SC03A_SeedFlights
{
    public async Task RunAsync()
    {
        var eventStore = new CosmosEventStore(
            "StarTours.Domain.Events.{0}, StarTours",
            Config.CosmosDb.Sql.HostName,
            Config.CosmosDb.Sql.AuthorizationKey,
            Config.CosmosDb.Sql.DatabaseId);

        var flights = new List<Flight>
        {
            new Flight("42", "thx1138", "naboo"),
            new Flight("42", "naboo", "thx1138"),
            new Flight("84", "thx1138", "kessel"),
            new Flight("840", "kessel", "thx1138"),
            new Flight("321", "thx1138", "bespin"),
            new Flight("3210", "bespin", "thx1138"),
            new Flight("843", "thx1138", "tatooine"),
            new Flight("8430", "tatooine", "thx1138"),
            new Flight("1219", "thx1138", "hoth"),
            new Flight("12190", "hoth", "thx1138"),
            new Flight("14010", "coruscant", "thx1138"),
            new Flight("1430", "thx1138", "yaviniv"),
            new Flight("14300", "yaviniv", "thx1138"),
            new Flight("2381", "thx1138", "alderaan"),
            new Flight("23810", "alderaan", "thx1138"),
            new Flight("346", "endor", "hoth"),
            new Flight("1832", "endor", "yaviniv"),
            new Flight("18320", "yaviniv", "endor"),
            new Flight("431", "coruscant", "tatooine"),
            new Flight("482", "yaviniv", "alderaan"),
            new Flight("923", "coruscant", "alderaan"),
            new Flight("9230", "alderaan", "coruscant")
        };

        var endorExpress = new Flight("45", "thx1138", "endor");
        endorExpress.AssignRoute(new[] { new FlightLeg("thx1138", "endor", "SP-72")});
        endorExpress.Divert(new[] { new FlightLeg("thx1138", "tatooine", "SP-72")});

        flights.Add(endorExpress);

        foreach (var flight in flights)
        {
            await eventStore.AppendToStreamAsync(
                $"flight:{flight.FlightNumber}",
                flight.Version,
                flight.PendingChanges);
        }

        Console.WriteLine($"Added {flights.Count} flights.");
    }
}
