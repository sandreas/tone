using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Metadata.Extensions;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class TaggerComposite : ITagger
{
    private readonly IEnumerable<string> _order = Array.Empty<string>();
    
    private const string PlaceHolderStar = "*";
    public List<INamedTagger> Taggers { get; } = new();
    public IEnumerable<INamedTagger> OrderedTaggers => OrderTaggers(_order, Taggers.ToArray());

    public TaggerComposite(params INamedTagger[] taggers)
    {
        Taggers.AddRange(taggers);
    }

    public TaggerComposite(IEnumerable<string> order, params INamedTagger[] taggers) : this(
        taggers)
    {
        _order = order;
    }

    private static IEnumerable<INamedTagger> OrderTaggers(IEnumerable<string> order, INamedTagger[] taggers)
    {
        var orderArray = order.ToArray();
        if(orderArray.Length == 0)        {
            return taggers;
        }
            
        var taggerNames = _normalizeTaggerNames(orderArray);
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

            // todo: normalize tagger names so that Tagger-Suffix is removed, instead of appended (because ScriptTaggers may don't work like that)
            var taggerToAdd = taggers.FirstOrDefault(t =>
                string.Equals(t.Name.TrimSuffix("Tagger"), taggerName.TrimSuffix("Tagger"), StringComparison.InvariantCultureIgnoreCase));
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

    public async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata=null)
    {
        if(originalMetadata == null){
            originalMetadata = new ToneJsonMeta(); // todo: rename class to something more generic
            originalMetadata.OverwriteProperties(metadata);
        }
        var error = "";
        foreach (var tagger in OrderedTaggers)
        {
            var result = await tagger.UpdateAsync(metadata, originalMetadata);
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