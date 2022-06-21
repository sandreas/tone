using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class TaggerComposite : ITagger
{
    private const string PlaceHolderStar = "*";
    public List<ITagger> Taggers { get; } = new();
    

    public TaggerComposite(params INamedTagger[] taggers)
    {
        Taggers.AddRange(taggers);
    }

    public TaggerComposite(IEnumerable<string> order, params INamedTagger[] taggers) : this(
        OrderTaggers(order, taggers))
    {
        
    }

    private static INamedTagger[] OrderTaggers(IEnumerable<string> order, INamedTagger[] taggers)
    {
        
        var taggerNames = _normalizeTaggerNames(order);
        var nonStarNames = taggerNames.Where(o => o != PlaceHolderStar).ToArray();
        if (nonStarNames.Length == 0)
        {
            return taggers;
        }
        var orderedTaggers = new List<INamedTagger>();
        foreach (var taggerName in taggerNames)
        {
            if (taggerName == PlaceHolderStar)
            {
                orderedTaggers.AddRange(taggers.Where(t => !nonStarNames.Any(n => string.Equals(t.Name, n, StringComparison.InvariantCultureIgnoreCase))));
                continue;
            }

            var taggerToAdd = taggers.FirstOrDefault(t =>
                string.Equals(t.Name, taggerName, StringComparison.InvariantCultureIgnoreCase));
            if (taggerToAdd != null)
            {
                orderedTaggers.Add(taggerToAdd);
            }
        }

        return orderedTaggers.ToArray();
    }

    private static string[] _normalizeTaggerNames(IEnumerable<string> order)
    {
        return order.SelectMany(o => o.Split(",")).Select(o =>
        {
            var taggerName = o.Trim();
            if (taggerName != PlaceHolderStar && !taggerName.ToLowerInvariant().EndsWith("Tagger"))
            {
                taggerName += "Tagger";
            }
            return taggerName;
        }).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
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