namespace tone.Metadata.Taggers;

public interface INamedTagger: ITagger
{
    public string Name { get; }
}