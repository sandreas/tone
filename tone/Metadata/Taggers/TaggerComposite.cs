using System.Collections.Generic;

namespace tone.Metadata.Taggers;

public class TaggerComposite: ITagger
{
    public IEnumerable<ITagger> Updaters { get; } = new List<MetadataTagger>();
    
    public void Update(IMetadata metadata)
    {
        foreach (var updater in Updaters)
        {
            updater.Update(metadata);
        }
    }
}