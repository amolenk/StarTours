using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventStore
{
    public class EventEnvelope
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("streamId")]
        public string StreamId { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("eventData")]
        public JObject EventData { get; set; }

        public IEvent GetEvent(IEventTypeResolver eventTypeResolver)
        {
            Type eventType = eventTypeResolver.GetEventType(EventType);
            
            return (IEvent)EventData.ToObject(eventType);
        }
    }
}