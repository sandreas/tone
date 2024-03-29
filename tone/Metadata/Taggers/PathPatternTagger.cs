using System;
using System.Threading.Tasks;
using GrokNet;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Matchers;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class PathPatternTagger : INamedTagger
{
    public string Name => nameof(PathPatternTagger);

    private readonly PathPatternMatcher _pathPatternMatcher;

    public PathPatternTagger(PathPatternMatcher pathPatternMatcher)
    {
        _pathPatternMatcher = pathPatternMatcher;
    }

    public async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata = null)
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