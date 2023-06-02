using StarTours.Domain.Events;
using StarTours.Shared.EventStore;

namespace StarTours.Domain
{
    public class Flight
    {
        private List<FlightLeg> _route = new();

        public string FlightNumber { get; private set; }

        public string OriginId { get; private set; }

        public string DestinationId { get; private set; }

        public bool IsCanceled { get; private set; }

        public IReadOnlyCollection<FlightLeg> Route => _route.AsReadOnly();

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
                ApplyEvent(@event, isReplay: true);
            }
        }

        public void AssignRoute(IEnumerable<FlightLeg> route)
        {
            if (IsCanceled)
            {
                throw new ArgumentException("Cannot assign a route to a canceled flight.");
            }

            if (_route.Any())
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
            if (IsCanceled)
            {
                throw new ArgumentException("Cannot divert a canceled flight.");
            }

            if (!_route.Any())
            {
                throw new ArgumentException("Cannot divert a flight that doesn't have a route assigned.");
            }

            ApplyEvent(new FlightDiverted
            {
                FlightNumber = FlightNumber,
                OriginId = OriginId,
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
            if (isReplay)
            {
                Version++;
            }
            else
            {
                PendingChanges.Add(@event);
            }

            switch (@event)
            {
                case FlightScheduled flightScheduled:
                    HandleEvent(flightScheduled);
                    break;

                case FlightDiverted flightDiverted:
                    HandleEvent(flightDiverted);
                    break;

                case FlightRouteAssigned flightRouteAssigned:
                    HandleEvent(flightRouteAssigned);
                    break;

                case FlightCanceled flightCanceled:
                    HandleEvent(flightCanceled);
                    break;
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
            _route.Clear();
            DestinationId = flightDiverted.NewDestinationId;
        }

        private void HandleEvent(FlightRouteAssigned flightRouteAssigned)
        {
            _route.AddRange(flightRouteAssigned.Legs);
        }

        private void HandleEvent(FlightCanceled _)
        {
            _route.Clear();
            IsCanceled = true;
        }

        #region Snapshots

        public record Snapshot(
            string FlightNumber,
            string OriginId,
            string DestinationId,
            List<FlightLeg> Route,
            bool IsCanceled);

        public Flight(Snapshot snapshot, int snapshotVersion, IEnumerable<IEvent> events)
        {
            FlightNumber = snapshot.FlightNumber;
            OriginId = snapshot.OriginId;
            DestinationId = snapshot.DestinationId;
            IsCanceled = snapshot.IsCanceled;
            _route = snapshot.Route;
            Version = snapshotVersion;

            // Replay events to get current state.
            foreach (var @event in events)
            { 
                ApplyEvent(@event, isReplay: true);
            }
        }

        public Snapshot CreateSnapshot() => new Snapshot(
            FlightNumber,
            OriginId,
            DestinationId,
            _route,
            IsCanceled);

        #endregion
    }
}