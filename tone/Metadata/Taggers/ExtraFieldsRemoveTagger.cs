using System.Collections.Generic;
using System.IO.Abstractions;

namespace tone.Metadata.Taggers;

public class ExtraFieldsRemoveTagger : ITagger
{
    private readonly IReadOnlyList<string> _removeExtraFields;

    public ExtraFieldsRemoveTagger(IReadOnlyList<string> removeExtraFields)
    {
        _removeExtraFields = removeExtraFields;
    }

    public void Update(IMetadata metadata)
    {
        if (metadata.AdditionalFields == null)
        {
            return;
        }

        foreach (var field in _removeExtraFields)
        {
            if (metadata.AdditionalFields.ContainsKey(field))
            {
                metadata.AdditionalFields.Remove(field);
            }
        }
    }
}