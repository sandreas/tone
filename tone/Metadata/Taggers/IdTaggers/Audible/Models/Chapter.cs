using System.Collections.Generic;
using Newtonsoft.Json;

namespace tone.Metadata.Taggers.IdTaggers.Audible.Models;

public class Chapter
{
    [JsonProperty("length_ms")]
    public long LengthMs { get; set; }
    
    [JsonProperty("start_offset_ms")]
    public long StartOffsetMs { get; set; }
    
    [JsonProperty("start_offset_sec")]
    public long StartOffsetSec { get; set; }

    public string Title { get; set; } = "";
    
    public List<Chapter> Chapters { get; set; } = new();

}