using System.Collections.Generic;
using Newtonsoft.Json;

namespace tone.Metadata.Taggers.IdTaggers.Audible.Models;

public class ChapterInfo
{
    public long BrandIntroDurationMs { get; set; }
    public long BrandOutroDurationMs { get; set; }
    [JsonProperty("is_accurate")]
    public bool IsAccurate { get; set; }
    [JsonProperty("runtime_length_ms")]
    public long RuntimeLengthMs { get; set; }
    [JsonProperty("runtime_length_sec")]
    public long RuntimeLengthSec { get; set; }
    public List<Chapter> Chapters { get; set; } = new();

}