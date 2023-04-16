using Newtonsoft.Json;
using tone.Metadata.Taggers.IdTaggers.Audible.Models;

namespace tone.Metadata.Taggers.IdTaggers.Audible;

public class AudibleChaptersResponse
{
    [JsonProperty("content_metadata")]
    public ContentMetadata CategoryLadders { get; set; } = new();
}