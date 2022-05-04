using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class M4BFillUpTagger : TaggerBase
{
    // private const string RomanLetters = "IVXLCDM";

    public override async Task<Status<string>> UpdateAsync(IMetadata metadata)
    {
        if (!IsM4B(metadata))
        {
            return await Task.FromResult(Ok());
        }
        metadata.Album ??= metadata.Title;
        metadata.Title ??= metadata.Album;
        metadata.ItunesMediaType ??= ItunesMediaType.Audiobook;
        metadata.ItunesPlayGap ??= ItunesPlayGap.NoGap;
        metadata.Narrator ??= metadata.Composer;
        metadata.Comment ??= metadata.LongDescription ?? metadata.Description;

        if (ShouldUpdateSortTitle(metadata))
        {
            // If ALBUM only, then %Title%
            //     If ALBUM and SUBTITLE, then %Title% - %Subtitle%
            //     If Series, then %Series% %Series-part% - %Title%
            metadata.SortTitle = MultiConcat(metadata.MovementName, " ", metadata.Movement, " - " + metadata.Title);
            metadata.SortAlbum ??= metadata.SortTitle;
            metadata.AdditionalFields["shwm"] = "1"; // show movement
        }
        
        return await Task.FromResult(Ok());
    }

    private static bool ShouldUpdateSortTitle(IMetadata metadata)
    {
        return HasMovement(metadata) &&
               (string.IsNullOrEmpty(metadata.SortTitle) || metadata.SortTitle == metadata.Title);
    }

    private static bool HasMovement(IMetadata metadata)
    {
        return !string.IsNullOrEmpty(metadata.MovementName) || !string.IsNullOrEmpty(metadata.Movement);
    }

    private static string MultiConcat(params string?[] concatStrings)
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

    private static bool IsM4B(IMetadata metadata)
    {
        return metadata.ItunesMediaType == ItunesMediaType.Audiobook ||
               metadata.Path != null && metadata.Path.EndsWith(".m4b");
    }

}