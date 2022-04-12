using System.Collections.Generic;
using System.Threading.Tasks;
using OperationResult;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class TaggerComposite : ITagger
{
    public List<ITagger> Taggers { get; } = new();

    public async Task<Status<string>> Update(IMetadata metadata)
    {
        foreach (var tagger in Taggers)
        {
            var result = await tagger.Update(metadata);
            if (!result)
            {
                return Error(result.Error);
            }
        }

        return await Task.FromResult(Ok());
    }
}