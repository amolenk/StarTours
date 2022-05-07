using StarTours.Domain.Events;
using StarTours.Shared;
using StarTours.Shared.EventStore;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace StarTours.Scenarios;

public class SC003B_FlightStatusProjection
{
    private const string EventTypeFormat = "StarTours.Domain.Events.{0}, Demo";
    private const string PartitionKey = "flightStatus";

    private CosmosClient _client;
    private Container _viewContainer;

    public async Task RunAsync()
    {
        _client = new CosmosClient(Config.CosmosDb.ConnectionString);

        Container eventContainer = _client.GetContainer(Config.CosmosDb.DatabaseId, Config.CosmosDb.Containers.Events);
        Container leaseContainer = _client.GetContainer(Config.CosmosDb.DatabaseId, Config.CosmosDb.Containers.Leases);
        _viewContainer = _client.GetContainer(Config.CosmosDb.DatabaseId, Config.CosmosDb.Containers.Views);

        // Process from beginning each time.
        var processorName = Guid.NewGuid().ToString();

        ChangeFeedProcessor processor = eventContainer
            .GetChangeFeedProcessorBuilder<EventDocument>(processorName, HandleChangesAsync)
            .WithInstanceName(nameof(SC003B_FlightStatusProjection))
            .WithLeaseContainer(leaseContainer)
            .WithStartTime(new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .WithErrorNotification(HandleErrorAsync)
            .Build();

        await processor.StartAsync();

        Console.ReadKey();

        await processor.StopAsync();


    }

    private Task HandleErrorAsync(string leaseToken, Exception exception)
    {
        Console.WriteLine(exception);
        return Task.CompletedTask;
    }

    private async Task HandleChangesAsync(IReadOnlyCollection<EventDocument> events, CancellationToken cancellationToken)
    {
        var flightEvents = events.Where(doc => doc.StreamId.StartsWith("flight:") && doc.Id.StartsWith("event:"));

        foreach (var eventDocument in flightEvents)
        {
            var actualEvent = eventDocument.GetEvent(EventTypeFormat);

            Console.WriteLine("Got change: " + eventDocument.StreamId + "/" + eventDocument.Id);

            switch (actualEvent)
            {
                case FlightScheduled flightScheduled:
                    await HandleEventAsync(flightScheduled, eventDocument.Version);
                    break;

                case FlightDiverted flightDiverted:
                    await HandleEventAsync(flightDiverted, eventDocument.Version);
                    break;

                case FlightCanceled flightCanceled:
                    await HandleEventAsync(flightCanceled, eventDocument.Version);
                    break;
            }
        }
    }

    private async Task HandleEventAsync(FlightScheduled flightScheduled, int version)
    {
        var document = new FlightStatusDocument
        {
            Id = flightScheduled.FlightNumber,
            PartitionKey = "flightStatus",
            FlightStatus = "Scheduled",
            Version = version
        };
        
        // CreateItemAsync is good enough here, but upsert is handy for demos ;-)
        await _viewContainer.UpsertItemAsync(document);
    }

    private async Task HandleEventAsync(FlightDiverted flightDiverted, int version)
    {
        FlightStatusDocument document = await _viewContainer.ReadItemAsync<FlightStatusDocument>(
            flightDiverted.FlightNumber,
            new("flightStatus"));

        document.FlightStatus = "Diverted";
        document.Version = version;

        await _viewContainer.UpsertItemAsync(document);
    }

    private async Task HandleEventAsync(FlightCanceled flightCanceled, int version)
    {
        FlightStatusDocument document = await _viewContainer.ReadItemAsync<FlightStatusDocument>(
            flightCanceled.FlightNumber,
            new("flightStatus"));

        document.FlightStatus = "Canceled";
        document.Version = version;

        await _viewContainer.UpsertItemAsync(document);
    }

    public class FlightStatusDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
            
        [JsonProperty(PropertyName = "status")]
        public string FlightStatus { get; set; }

        [JsonProperty(PropertyName = "version")]
        public int Version { get; set; }
    }
}
