using System;
using System.Collections.Generic;
using System.Linq;
using GrokNet;
using tone.Common.Extensions.String;

namespace tone.Matchers;

public class PathPatternMatcher
{
    public bool HasPatterns => Patterns.Count > 0;
    private IList<(string patternAsString, Grok)> Patterns { get; set; }

    public PathPatternMatcher(IEnumerable<(string patternAsString, Grok)> patterns)
    {
        Patterns = patterns.ToList();
    }
    public bool TryMatchSinglePattern(string path, out (string patternAsString, Grok) match, Action<string, Grok, GrokResult>? resultHandler = null)
    {
        match = ("", new Grok("")); // this is a dummy and should not be used
        if (!Patterns.Any())
        {
            return false;
        }
        
        var normalizedMetadataPath = path.TrimDirectorySeparatorEnd();
        if (normalizedMetadataPath == "")
        {
            return false;
        }
        foreach (var (pattern, grok) in Patterns)
        {
            var results = grok.Parse(normalizedMetadataPath);
            if (results == null || results.Count == 0)
            {
                continue;
            }

            match = (pattern, grok);
            resultHandler?.Invoke(pattern, grok, results);
            return true;
        }

        return false;
    }
}