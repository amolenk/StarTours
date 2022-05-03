namespace Demo.Domain.Events;

public class FlightScheduled : EventBase
{
    public string FlightNumber { get; set; }
}
