using StarTours.Shared.EventStore;

namespace StarTours.Domain.Events;

public class FlightCanceled : IEvent
{
    public string FlightNumber { get; set; }
}
