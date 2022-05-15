using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StarTours.Shared.EventStore;

public class SnapshotDocument
{
    [JsonProperty("id")]
    public string Id => "snapshot";

    [JsonProperty("streamId")]
    public string StreamId { get; set; }

    [JsonProperty("version")]
    public int Version { get; set; }

    [JsonProperty("state")]
    public JObject State { get; set; }

    public static SnapshotDocument Create(string streamId, int version, object snapshot)
        => new SnapshotDocument
        {
            StreamId = streamId,
            Version = version,
            State = JObject.FromObject(snapshot)
        };

    public T Deserialize<T>() => State.ToObject<T>();
}
