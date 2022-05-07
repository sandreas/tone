using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrokNet;
using OperationResult;
using tone.Common.Extensions.String;
using static OperationResult.Helpers;


namespace tone.Metadata.Taggers;

public class PathPatternTagger : TaggerBase
{
    private readonly IEnumerable<Grok> _grokPatterns;

    public PathPatternTagger(IEnumerable<Grok> grokPatterns)
    {
        _grokPatterns = grokPatterns;
    }


    public override async Task<Status<string>> UpdateAsync(IMetadata metadata)
    {
        if (!_grokPatterns.Any())
        {
            return await Task.FromResult(Ok());
        }

        var normalizedMetadataPath = (metadata.Path ?? "").TrimDirectorySeparatorEnd();
        if (normalizedMetadataPath == "")
        {
            return Error("metadata path cannot be empty");
        }

        var matchesFound = false;
        foreach (var pattern in _grokPatterns)
        {
            var results = pattern.Parse(normalizedMetadataPath);
            if (results.Count == 0)
            {
                continue;
            }

            matchesFound = true;
            MapResults(results, metadata);
        }

        if (!matchesFound)
        {
            // var privateInt = test.GetType().GetProperty("PrivateInt", BindingFlags.Instance | BindingFlags.NonPublic);
            return Error(
                $"Did not find any matches for given path patterns (path: {normalizedMetadataPath}");
        }

        return Ok();
    }

    private void MapResults(GrokResult results, IMetadata metadata)
    {
        foreach (var grokItem in results)
        {
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