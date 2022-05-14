using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StarTours.Shared.EventStore;

public class EventDocument
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("streamId")]
    public string StreamId { get; set; }

    [JsonProperty("version")]
    public int Version { get; set; }

    [JsonProperty("type")]
    public string EventType { get; set; }

    [JsonProperty("data")]
    public JObject EventData { get; set; }

    public static EventDocument Create(string streamId, int version, IEvent @event)
        => new EventDocument
        {
            Id = $"event:{version}",
            StreamId = streamId,
            Version = version,
            EventType = @event.GetType().Name,
            EventData = JObject.FromObject(@event)
        };

    public IEvent Deserialize(string eventTypeFormat)
    {
        var eventTypeName = string.Format(eventTypeFormat, EventType);
        var eventType = Type.GetType(eventTypeName, true);

        return (IEvent)EventData.ToObject(eventType);
    }
}
