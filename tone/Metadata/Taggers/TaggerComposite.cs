using System.Collections.Generic;
using System.Threading.Tasks;
using OperationResult;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class TaggerComposite : ITagger
{
    public List<ITagger> Taggers { get; } = new();

    public TaggerComposite(params ITagger[] taggers)
    {
        Taggers.AddRange(taggers);
    }
    
    public async Task<Status<string>> UpdateAsync(IMetadata metadata)
    {
        var error = "";
        foreach (var tagger in Taggers)
        {
            var result = await tagger.UpdateAsync(metadata);
            if (!result)
            {
                error += result.Error;
            }
        }

        if (error != "")
        {
            return Error(error);
        }
        return await Task.FromResult(Ok());
    }
}