using Demo.Domain.Events;
using EventStore;

namespace Demo.Domain
{
    public class Flight
    {
        public string FlightNumber { get; private set; }

        internal int Version = 0;

        internal List<IEvent> PendingChanges { get; } = new();

        public Flight(string flightNumber)
        {
            ApplyEvent(new FlightScheduled
            {
                FlightNumber = flightNumber
            });
        }

        public Flight(IEnumerable<IEvent> events)
        {
            // Replay events to get current state.
            foreach (var @event in events)
            { 
                ApplyEvent(@event, true);
            }
        }

        private void ApplyEvent(IEvent @event, bool isReplay = false)
        {
            switch (@event)
            {
                case FlightScheduled flightScheduled:
                    HandleEvent(flightScheduled);
                    break;
            }

            if (isReplay)
            {
                Version++;
            }
            else
            {
                PendingChanges.Add(@event);
            }
        }

        private void HandleEvent(FlightScheduled flightScheduled)
        {
            FlightNumber = flightScheduled.FlightNumber;
        }
    }
}