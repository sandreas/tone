using Newtonsoft.Json;

namespace tone.Metadata.Taggers.IdTaggers.Audible.Models;

public class ContentMetadata
{
    [JsonProperty("chapter_info")]
    public ChapterInfo ChapterInfo { get; set; } = new();
}