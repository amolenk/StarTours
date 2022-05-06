//using Demo.Domain.Events;
//using Demo.Shared;
//using EventStore;
//using Microsoft.Azure.Cosmos;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Newtonsoft.Json;
//using System.Net;
//using static Microsoft.Azure.Cosmos.Container;

//namespace Demo.Scenarios
//{
//    public class SC000_DivertedFlightCountProjection : IDemoScenario
//    {
//        private const string EventTypeFormat = "Demo.Domain.Events.{0}, Demo";
//        private const string PartitionKey = "divertedFlights";

//        private CosmosClient _client;
//        private Container _viewContainer;

//        [TestMethod]
//        public async Task RunAsync()
//        {
//            _client = new CosmosClient(Config.CosmosDb.ConnectionString);

//            Container eventContainer = _client.GetContainer(Config.CosmosDb.DatabaseId, Config.CosmosDb.Containers.Events);
//            Container leaseContainer = _client.GetContainer(Config.CosmosDb.DatabaseId, Config.CosmosDb.Containers.Leases);
//            _viewContainer = _client.GetContainer(Config.CosmosDb.DatabaseId, Config.CosmosDb.Containers.Views);

//            ChangeFeedProcessor processor = eventContainer
//                .GetChangeFeedProcessorBuilder<EventDocument>(Guid.NewGuid().ToString(), HandleChangesAsync)
//                .WithInstanceName(nameof(SC000_FlightStatusProjection))
//                .WithLeaseContainer(leaseContainer)
//                .WithStartTime(new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc))
//                .WithErrorNotification(HandleErrorAsync)
//                .Build();

////            new ChangeFeedHandler<string>()

//            await processor.StartAsync();

//            Console.ReadKey();

//            await processor.StopAsync();
//        }

//        private Task HandleErrorAsync(string leaseToken, Exception exception)
//        {
//            Console.WriteLine(exception);
//            return Task.CompletedTask;
//        }

//        private async Task HandleChangesAsync(ChangeFeedProcessorContext context, IReadOnlyCollection<EventDocument> events, CancellationToken cancellationToken)
//        {
//            context.LeaseToken.

//            var flightEvents = events.Where(doc => doc.StreamId.StartsWith("flight:") && doc.Id.StartsWith("event:"));

//            foreach (var eventDocument in flightEvents)
//            {
//                var actualEvent = eventDocument.GetEvent(EventTypeFormat);

//                Console.WriteLine("Got change: " + eventDocument.StreamId + "/" + eventDocument.Id);

//                await HandleEventAsync(actualEvent);
//            }
//        }

//        private Task HandleEventAsync(IEvent @event)
//        {
//            switch (@event)
//            {
//                case FlightScheduled flightScheduled:
//                    return UpdateDocument(flightScheduled.FlightNumber, doc =>
//                    {
//                        doc.Count += 1;
//                    });
//            }

//            return Task.CompletedTask;
//        }

//        private async Task UpdateDocument(string id, Action<DivertedFlightCountDocument> updateDocument)
//        {
//            var (document, etag) = await LoadFlightStatusDocumentAsync(id);

//            updateDocument(document);

//            var upsertOptions = new ItemRequestOptions
//            {
//                IfMatchEtag = etag
//            };

//            await _viewContainer.UpsertItemAsync(document, requestOptions: upsertOptions);
//        }

//        private async Task<(DivertedFlightCountDocument, string)> LoadFlightStatusDocumentAsync(string flightNumber)
//        {
//            try
//            {
//                var response = await _viewContainer.ReadItemAsync<DivertedFlightCountDocument>("count", new(PartitionKey));
//                return (response.Resource, response.ETag);
//            }
//            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
//            {
//                var newDocument = new DivertedFlightCountDocument
//                {
//                    Id = "count",
//                    PartitionKey = PartitionKey
//                };

//                return (newDocument, null);
//            }
//        }

       


//        //private async Task HandleEventAsync(FlightScheduled flightScheduled)
//        //{
//        //    var container = _client.GetContainer(Config.CosmosDb.DatabaseId, Config.CosmosDb.Containers.Views);
//        //    var partitionKey = "flightstatus";

//        //    FlightStatusDocument flightStatusDocument = null;

//        //    try
//        //    {
//        //        flightStatusDocument = await container.ReadItemAsync<FlightStatusDocument>(flightScheduled.FlightNumber, new(partitionKey));
//        //    }
//        //    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
//        //    {
//        //        flightStatusDocument = new()
//        //        {
//        //            Id = flightScheduled.FlightNumber,
//        //            PartitionKey = partitionKey
//        //        };
//        //    }

//        //    flightStatusDocument.FlightStatus = "Scheduled";

//        //    await container.UpsertItemAsync(flightStatusDocument);
//        //}

//        public class DivertedFlightCountDocument
//        {
//            [JsonProperty(PropertyName = "id")]
//            public string Id { get; set; }

//            [JsonProperty(PropertyName = "partitionKey")]
//            public string PartitionKey { get; set; }
            
//            [JsonProperty(PropertyName = "count")]
//            public int Count { get; set; }
//        }

//    }
//}
