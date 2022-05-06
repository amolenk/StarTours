using EventStore;

namespace Demo.Domain.Events;

public class FlightCanceled : IEvent
{
    public string FlightNumber { get; set; }
}
