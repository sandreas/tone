using System.Collections.Generic;
using System.Threading.Tasks;
using OperationResult;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class ExtraFieldsTagger : ITagger
{
    private readonly IReadOnlyList<string> _extraFields;

    public ExtraFieldsTagger(IReadOnlyList<string> extraFields)
    {
        _extraFields = extraFields;
    }

    public async Task<Status<string>> Update(IMetadata metadata)
    {
        if (_extraFields.Count % 2 != 0)
        {
            return Error("metadata.AdditionalFields has to contain an even number of values (<fieldname> <fieldvalue> ...)");
        }

        var index = 0;
        var key = "";
        foreach (var field in _extraFields)
        {
            if (index++ % 2 == 0)
            {
                key = field;
                continue;
            }

            if (metadata.AdditionalFields.ContainsKey(key))
            {
                continue;
            }

            metadata.AdditionalFields[key] = field;
        }

        return await Task.FromResult(Ok());
    }
}