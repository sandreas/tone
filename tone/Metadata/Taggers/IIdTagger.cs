namespace tone.Metadata.Taggers;

public interface IIdTagger:ITagger
{
    public string Id { get; set; }
    public bool SupportsId(string id);
}