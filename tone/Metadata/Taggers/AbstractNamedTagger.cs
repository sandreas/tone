using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using Sandreas.AudioMetadata;

namespace tone.Metadata.Taggers;

public abstract class AbstractNamedTagger: INamedTagger
{
    public abstract Task<Status<string>> UpdateAsync(IMetadata metadata);
    public abstract string Name { get; }
    
    protected static bool HasMovementOrPart(IMetadata metadata)
    {
        return !string.IsNullOrEmpty(metadata.MovementName) || !string.IsNullOrEmpty(metadata.Part);
    }

    protected static string MultiConcat(params string?[] concatStrings)
    {
        var values = concatStrings.Where((_, i) => i % 2 == 0).ToImmutableArray();
        var separator = concatStrings.Where((_, i) => i % 2 != 0).ToImmutableArray();
        var concatValues = new List<string?>();
        for (var i = 0; i < values.Length; i++)
        {
            if (string.IsNullOrEmpty(values[i]))
            {
                continue;
            }

            var separatorIndex = i - 1;
            if (separatorIndex >= 0 && separatorIndex < separator.Length && concatValues.Count > 0)
            {
                concatValues.Add(separator[separatorIndex]);
            }

            concatValues.Add(values[i]);
        }

        return string.Join("", concatValues);
    }
    protected static bool ShouldUpdateSortTitle(IMetadata metadata)
    {
        return HasMovementOrPart(metadata) &&
               (string.IsNullOrEmpty(metadata.SortTitle) || metadata.SortTitle == metadata.Title);
    }
/*
    private static void ExtractSeriesFromSortProperties(IMetadata metadata)
    {
        if (string.IsNullOrEmpty(metadata.SortTitle))
        {
            return;
        }

        if (metadata.SortTitle != metadata.SortAlbum)
        {
            return;
        }

        var suffix = " - " + metadata.Title;
        if (!metadata.SortTitle.EndsWith(suffix))
        {
            return;
        }

        var seriesEndPos = metadata.SortTitle.Length - suffix.Length;
        var seriesString = metadata.SortTitle[..seriesEndPos].Trim();
        if (seriesString == "")
        {
            return;
        }

        var seriesStringSplit = seriesString.Split(null).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        var seriesPartString = seriesStringSplit.LastOrDefault() ?? "";
        if (seriesStringSplit.Length > 1 && IsPartNumber(seriesPartString))
        {
            seriesString = seriesString[..^seriesPartString.Length].Trim();
        }

        metadata.MovementName = seriesString;
        metadata.Movement = seriesPartString == "" ? null : seriesPartString;
    }

    private static bool IsPartNumber(string seriesPartString)
    {
        return IsRomanNumber(seriesPartString) ||
               StartsAndEndsWithDigit(seriesPartString);
    }

    private static bool IsRomanNumber(string str)
    {
        return str.All(c => RomanLetters.Contains(c));
    }

    private static bool StartsAndEndsWithDigit(string str)
    {
        return char.IsDigit(str.First()) && char.IsDigit(str.Last());
    }

    private void UpdateSortProperties(IMetadata metadata)
    {
        if (metadata.MovementName != null && metadata.SortTitle == metadata.Title)
        {
            metadata.SortTitle = (metadata.MovementName + " " + metadata.Movement).Trim() + " - " + metadata.Title;
        }

        metadata.SortAlbum ??= metadata.SortTitle;
    }
    */

    protected static bool IsM4B(IMetadata metadata)
    {
        return metadata.ItunesMediaType == ItunesMediaType.Audiobook ||
               metadata.Path != null && metadata.Path.EndsWith(".m4b");
    }
}