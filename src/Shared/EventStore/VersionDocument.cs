using Newtonsoft.Json;

namespace StarTours.Shared.EventStore;

public class VersionDocument
{
    [JsonProperty("id")]
    public string Id => "version";

    [JsonProperty("streamId")]
    public string StreamId { get; set; }

    [JsonProperty("version")]
    public int Version { get; set; }

    public static VersionDocument Create(string streamId, int version)
        => new VersionDocument
        {
            StreamId = streamId,
            Version = version
        };
}
