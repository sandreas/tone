using System.Collections.Generic;

namespace tone.Metadata.Taggers;

public class MetadataTagger : TaggerBase
{
    private readonly IMetadata _source;

    public MetadataTagger(IMetadata source)
    {
        _source = source;
    }

    public override void Update(IMetadata metadata)
    {
        TransferMetadataProperties(_source, metadata);
        TransferMetadataLists(_source, metadata);
    }

}