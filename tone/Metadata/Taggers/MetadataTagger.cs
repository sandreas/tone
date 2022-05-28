using System.Threading.Tasks;
using OperationResult;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class MetadataTagger : ITagger
{
    private readonly IMetadata _source;

    public MetadataTagger(IMetadata source)
    {
        _source = source;
    }

    public async Task<Status<string>> UpdateAsync(IMetadata metadata)
    {
        metadata.OverwritePropertiesWhenNotEmpty(_source);
        return await Task.FromResult(Ok());
    }
}