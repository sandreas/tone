using System.Collections.Generic;
using System.Threading.Tasks;
using OperationResult;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class TaggerComposite : ITagger
{
    public List<ITagger> Taggers { get; } = new();
    
    public async Task<Status<string>> UpdateAsync(IMetadata metadata)
    {
        foreach (var tagger in Taggers)
        {
            var result = await tagger.UpdateAsync(metadata);
            if (!result)
            {
                return Error(result.Error);
            }
        }

        return await Task.FromResult(Ok());
    }
}