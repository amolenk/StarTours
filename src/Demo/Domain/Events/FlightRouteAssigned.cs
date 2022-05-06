using EventStore;

namespace Demo.Domain.Events;

public class FlightRouteAssigned : IEvent
{
    public string FlightNumber { get; set; }

    public List<FlightLeg> Legs { get; set; }
}
