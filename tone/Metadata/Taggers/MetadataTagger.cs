using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using Sandreas.AudioMetadata;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class MetadataTagger : INamedTagger
{
    public string Name => nameof(MetadataTagger);

    private readonly IMetadata _source;

    public MetadataTagger(IMetadata source)
    {
        _source = source;
    }

    public async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata = null)
    {  
        var backupAdditionalFields = metadata.AdditionalFields.ToDictionary(entry => entry.Key,
            entry => entry.Value);
        metadata.OverwritePropertiesWhenNotEmpty(_source);
        foreach (var f in backupAdditionalFields.Where(f => !metadata.AdditionalFields.ContainsKey(f.Key)))
        {
            metadata.AdditionalFields.Add(f.Key, f.Value);
        }
        return await Task.FromResult(Ok());
    }
}