using System.Collections.Generic;
using System.Threading.Tasks;
using OperationResult;
using tone.Commands.Settings.Interfaces;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class AdditionalFieldsRemoveTagger : ITagger
{
    private readonly IReadOnlyList<string> _removeExtraFields;

    public AdditionalFieldsRemoveTagger(IReadOnlyList<string> removeExtraFields)
    {
        _removeExtraFields = removeExtraFields;
    }

    public AdditionalFieldsRemoveTagger(IRemoveAdditionalFieldsSettings removeSettings)
    {
        _removeExtraFields = removeSettings.RemoveAdditionalFields;
    }
    
    public async Task<Status<string>> UpdateAsync(IMetadata metadata)
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