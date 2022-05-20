using System.Collections.Generic;
using System.Threading.Tasks;
using OperationResult;
using tone.Metadata;
using tone.Metadata.Taggers;
using static OperationResult.Helpers;

namespace tone.Services;

public class TagService: ITagger
{
    public List<ITagger> Taggers { get; } = new();

    public TagService(ITagger metaTagger)
    {
        Taggers.Add(metaTagger);
    }
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