using Demo.Domain.Events;
using EventStore;

namespace Demo.Domain
{
    public class Flight
    {
        private List<FlightLeg> _legs;
        private bool _isCanceled;

        public string FlightNumber { get; private set; }

        public string OriginId { get; private set; }

        public string DestinationId { get; private set; }

        public IReadOnlyCollection<FlightLeg> Legs => _legs.AsReadOnly();

        internal int Version = 0;

        internal List<IEvent> PendingChanges { get; } = new();

        public Flight(string flightNumber, string originId, string destinationId)
        {
            ApplyEvent(new FlightScheduled
            {
                FlightNumber = flightNumber,
                OriginId = originId,
                DestinationId = destinationId
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

        public void AssignRoute(IEnumerable<FlightLeg> route)
        {
            if (_isCanceled)
            {
                throw new ArgumentException("Cannot assign a route to a canceled flight.");
            }

            if (_legs is not null)
            {
                throw new ArgumentException("Flight already has a planned route.");
            }

            ApplyEvent(new FlightRouteAssigned
            {
                FlightNumber = FlightNumber,
                Legs = route.ToList()
            });
        }

        public void Divert(IEnumerable<FlightLeg> newRoute)
        {
            if (_isCanceled)
            {
                throw new ArgumentException("Cannot divert a canceled flight.");
            }

            if (_legs is null)
            {
                throw new ArgumentException("Cannot divert a flight that doesn't have a route assigned.");
            }

            ApplyEvent(new FlightDiverted
            {
                FlightNumber = FlightNumber,
                NewDestinationId = newRoute.Last().DestinationId
            });

            ApplyEvent(new FlightRouteAssigned
            {
                FlightNumber = FlightNumber,
                Legs = newRoute.ToList()
            });
        }

        public void Cancel()
        {
            ApplyEvent(new FlightCanceled
            {
                FlightNumber = FlightNumber
            });
        }

        private void ApplyEvent(IEvent @event, bool isReplay = false)
        {
            switch (@event)
            {
                case FlightScheduled flightScheduled:
                    HandleEvent(flightScheduled);
                    break;

                case FlightDiverted flightDiverted:
                    HandleEvent(flightDiverted);
                    break;

                case FlightRouteAssigned flightLegAssigned:
                    HandleEvent(flightLegAssigned);
                    break;

                case FlightCanceled flightCanceled:
                    HandleEvent(flightCanceled);
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
            OriginId = flightScheduled.OriginId;
            DestinationId = flightScheduled.DestinationId;
        }

        private void HandleEvent(FlightDiverted flightDiverted)
        {
            DestinationId = flightDiverted.NewDestinationId;
            _legs = null;
        }

        private void HandleEvent(FlightRouteAssigned flightRouteAssigned)
        {
            _legs = flightRouteAssigned.Legs;
        }

        private void HandleEvent(FlightCanceled _)
        {
            _legs = null;
            _isCanceled = true;
        }
    }
}