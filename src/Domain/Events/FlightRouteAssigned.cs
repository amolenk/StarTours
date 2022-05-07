using StarTours.Shared.EventStore;

namespace StarTours.Domain.Events;

public class FlightRouteAssigned : IEvent
{
    public string FlightNumber { get; set; }

    public List<FlightLeg> Legs { get; set; }
}
