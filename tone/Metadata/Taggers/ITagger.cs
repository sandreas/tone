using System.IO.Abstractions;

namespace tone.Metadata.Taggers;

public interface ITagger
{
    public void Update(IMetadata metadata);
}