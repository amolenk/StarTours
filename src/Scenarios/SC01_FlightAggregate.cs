using StarTours.Domain;

namespace StarTours.Scenarios;

public class SC01_FlightAggregate
{
    public void Run()
    {
        // Schedule a new flight.
        var flight = new Flight("1401", "thx1138", "coruscant");

        // Assign a route.
        flight.AssignRoute(new[] { new FlightLeg("thx1138", "coruscant", "SP-38") });

        // Get the events from the aggregate.
        var events = flight.PendingChanges;

        Console.WriteLine("PENDING CHANGES:");
        foreach (var @event in events)
        {
            Console.WriteLine(@event);
        }

        Console.WriteLine("Press any key to rehydrate from events...");
        Console.ReadKey();
        Console.WriteLine();

        // Rehydrate aggregate from events.
        var replayedFlight = new Flight(events);

        Console.WriteLine("CURRENT FLIGHT STATUS (REPLAYED):");
        Console.WriteLine($"Version     = {replayedFlight.Version}");
        Console.WriteLine($"Origin      = {replayedFlight.OriginId}");
        Console.WriteLine($"Destination = {replayedFlight.DestinationId}");
        Console.WriteLine($"Leg(s)      = {replayedFlight.Route.Count()}");
    }
}
