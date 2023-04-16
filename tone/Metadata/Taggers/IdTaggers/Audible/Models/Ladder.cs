using Newtonsoft.Json;

namespace tone.Metadata.Taggers.IdTaggers.Audible.Models;

public class Ladder
{
    public string Id { get; set; } = "";
    
    [JsonProperty("is_new")]
    public bool IsNew { get; set; }
    public string Name { get; set; } = "";
    [JsonProperty("promote_upsell")]
    public bool PromoteUpsell { get; set; }
    [JsonProperty("suppress_download_option")]
    public bool SuppressDownloadOption { get; set; }
}