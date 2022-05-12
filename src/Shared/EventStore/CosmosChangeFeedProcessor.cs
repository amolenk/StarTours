using Microsoft.Azure.Cosmos;

// TODO Test a little bit: starting/stopping/canceling

namespace StarTours.Shared.EventStore;

public class CosmosChangeFeedProcessor
{
    private readonly CosmosClient _client;
    private readonly string _databaseId;
    private readonly string _feedContainerId;
    private readonly string _leaseContainerId;
    private ChangeFeedProcessor _processor;

    public CosmosChangeFeedProcessor(
        string connectionString, // Split into endpoint/authkey
        string databaseId,
        string feedContainerId,
        string leaseContainerId)
    {
        _client = new CosmosClient(connectionString);
        _databaseId = databaseId;
        _feedContainerId = feedContainerId;
        _leaseContainerId = leaseContainerId;
    }

    public Task StartAsync(
        string processorName,
        string instanceName,
        Container.ChangesHandler<EventDocument> onChanges)
    {
        var feedContainer = _client.GetContainer(_databaseId, _feedContainerId);
        var leaseContainer = _client.GetContainer(_databaseId, _leaseContainerId);

        // TODO 'monitoredContainer', move back into scenario

        _processor = feedContainer
            .GetChangeFeedProcessorBuilder<EventDocument>(processorName, onChanges)
            .WithInstanceName(instanceName)
            .WithLeaseContainer(leaseContainer)
            .WithStartTime(new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .WithErrorNotification(HandleErrorAsync)
            .Build();

        return _processor.StartAsync();
    }

    public Task StopAsync() => _processor?.StopAsync();

    private Task HandleErrorAsync(string leaseToken, Exception exception)
    {
        Console.WriteLine(exception);
        return Task.CompletedTask;
    }
}
