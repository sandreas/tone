using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Commands.Settings.Interfaces;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class RemoveTagger : INamedTagger
{
    public string Name => nameof(RemoveTagger);
    
    private readonly MetadataProperty[] _remove;
    private readonly string[] _removeAdditional;

    
    public RemoveTagger(IEnumerable<MetadataProperty> remove, IEnumerable<string> removeAdditional)
    {
        _remove = remove.ToArray();
        _removeAdditional = removeAdditional.ToArray();
    }

    public RemoveTagger(IRemoveTaggerSettings settings)
    {
        _remove = settings.Remove;
        _removeAdditional = settings.RemoveAdditionalFields;
    }

    public async Task<Status<string>> UpdateAsync(IMetadata metadata)
    {
        foreach (var property in _remove)
        {
            metadata.SetMetadataPropertyValue(property, null);
        }

        foreach (var key in _removeAdditional)
        {
            if(metadata.AdditionalFields.ContainsKey(key))
            {
                metadata.AdditionalFields.Remove(key);
            }
        }

        return await Task.FromResult(Ok());
    }

}


