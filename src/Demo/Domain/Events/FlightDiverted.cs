using EventStore;

namespace Demo.Domain.Events;

public class FlightDiverted : IEvent
{
    public string FlightNumber { get; set; }

    public string NewDestinationId { get; set; }
}
