using System;
using System.Threading.Tasks;
using GrokNet;
using OperationResult;
using tone.Matchers;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class PathPatternTagger : TaggerBase
{
    private readonly PathPatternMatcher _pathPatternMatcher;

    public PathPatternTagger(PathPatternMatcher pathPatternMatcher)
    {
        _pathPatternMatcher = pathPatternMatcher;
    }

    public override async Task<Status<string>> UpdateAsync(IMetadata metadata)
    {
        if (!_pathPatternMatcher.HasPatterns)
        {
            return await Task.FromResult(Ok());
        }
        if (_pathPatternMatcher.TryMatchSinglePattern(metadata.Path ?? "", out _,
                (_, _, result) => MapResults(result, metadata)))
        {
            return Ok();
        }

        return Error("Could not match string");
    }

    private static void MapResults(GrokResult? results, IMetadata metadata)
    {
        if (results == null)
        {
            return;
        }

        foreach (var grokItem in results)
        {
            if (grokItem.Key == "IgnoreDummy")
            {
                continue;
            }

            if (!Enum.TryParse(grokItem.Key, out MetadataProperty property))
            {
                continue;
            }

            var type = metadata.GetMetadataPropertyType(property);
            if (type == null)
            {
                continue;
            }

            var grokValueAsString = grokItem.Value.ToString() ?? "";
            metadata.SetMetadataPropertyValue(property, grokValueAsString, type);
        }
    }
}