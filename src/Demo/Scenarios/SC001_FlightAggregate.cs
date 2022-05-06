using Demo.Domain;

namespace Demo.Scenarios;

public class SC001_FlightAggregate
{
    public void Run()
    {
        // Schedule a new flight.
        var flight = new Flight("flightNr", "origin", "dest");

        // Assign a route.
        flight.AssignRoute(new[] { new FlightLeg("origin", "dest", "ship") });

        // Get the events from the aggregate.
        var events = flight.PendingChanges;

        // TODO Persist events in event store.

        // Rehydrate aggregate from events.
        flight = new Flight(events);

        flight.Divert(new[] { new FlightLeg("origin", "dest2", "ship2") });

        events = flight.PendingChanges;
    }
}
