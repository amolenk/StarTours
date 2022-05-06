using EventStore;

namespace Demo.Domain.Events;

public class FlightScheduled : IEvent
{
    public string FlightNumber { get; set; }

    public string OriginId { get; set; }

    public string DestinationId { get; set; }
}
