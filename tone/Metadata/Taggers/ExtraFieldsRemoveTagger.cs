using System.Collections.Generic;
using System.Threading.Tasks;
using OperationResult;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class ExtraFieldsRemoveTagger : ITagger
{
    private readonly IReadOnlyList<string> _removeExtraFields;

    public ExtraFieldsRemoveTagger(IReadOnlyList<string> removeExtraFields)
    {
        _removeExtraFields = removeExtraFields;
    }

    public async Task<Status<string>> Update(IMetadata metadata)
    {
        if (metadata.AdditionalFields != null)
        {
            foreach (var field in _removeExtraFields)
            {
                if (metadata.AdditionalFields.ContainsKey(field))
                {
                    metadata.AdditionalFields.Remove(field);
                }
            }
        }

        return await Task.FromResult(Ok());
    }
}