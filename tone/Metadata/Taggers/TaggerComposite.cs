using System.Collections.Generic;
using System.IO.Abstractions;

namespace tone.Metadata.Taggers;

public class TaggerComposite: ITagger
{
    public List<ITagger> Taggers { get; } = new();
    
    public void Update(IMetadata metadata)
    {
        foreach (var tagger in Taggers)
        {
            tagger.Update(metadata);
        }
    }
}