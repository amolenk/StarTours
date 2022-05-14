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

    [JsonProperty("data")]
    public JObject SnapshotData { get; set; }

    public static SnapshotDocument Create(string streamId, int version, object snapshot)
        => new SnapshotDocument
        {
            StreamId = streamId,
            Version = version,
            SnapshotData = JObject.FromObject(snapshot)
        };

    public T Deserialize<T>() => SnapshotData.ToObject<T>();
}
