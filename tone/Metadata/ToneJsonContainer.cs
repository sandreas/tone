namespace tone.Metadata;

public class ToneJsonContainer
{
    public ToneJsonAudio? Audio { get; set; }
    public ToneJsonMeta Meta { get; set; } = new();
    public ToneJsonFile? File { get; set; }
}