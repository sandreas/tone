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
        metadata.OverwritePropertiesWhenNotEmpty(_source);
        return await Task.FromResult(Ok());
    }
}