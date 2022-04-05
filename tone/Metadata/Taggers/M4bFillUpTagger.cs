using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class M4BFillUpTagger : TaggerBase
{
    private const string RomanLetters = "IVXLCDM";
    public override async Task<Status<string>> Update(IMetadata metadata)
    {
        if (IsM4b(metadata))
        {
            metadata.Album ??= metadata.Title;
            metadata.Title ??= metadata.Album;
            ExtractSeriesFromSortProperties(metadata);
            UpdateSortProperties(metadata);
        }


        return await Task.FromResult(Ok());
    }

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

    private bool IsM4b(IMetadata metadata)
    {
        if (metadata.ItunesMediaType != ItunesMediaType.Audiobook)
        {
            return false;
        }

        if (metadata.Path == null)
        {
            return false;
        }

        return metadata.Path.EndsWith(".m4b");
    }
}