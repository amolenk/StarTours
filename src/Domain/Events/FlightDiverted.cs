using StarTours.Shared.EventStore;

namespace StarTours.Domain.Events;

public class FlightDiverted : IEvent
{
    public string FlightNumber { get; set; }

    public string NewDestinationId { get; set; }
}
