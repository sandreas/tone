using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public override async Task<Status<string>> Update(IMetadata metadata)
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
            return Error("Did not find any matches for given path patterns.");
        }

        return Ok();
    }

    private void MapResults(GrokResult results, IMetadata metadata)
    {
        foreach (var grokItem in results)
        {
            metadata.Album = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Album), metadata.Album);
            metadata.AlbumArtist = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.AlbumArtist),
                metadata.AlbumArtist);
            metadata.Artist = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Artist), metadata.Artist);
            metadata.Bpm = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Bpm), metadata.Bpm);
            metadata.ChaptersTableDescription = MapResult(grokItem.Key, grokItem.Value,
                nameof(metadata.ChaptersTableDescription), metadata.ChaptersTableDescription);
            metadata.Composer = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Composer), metadata.Composer);
            metadata.Comment = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Comment), metadata.Comment);
            metadata.Conductor =
                MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Conductor), metadata.Conductor);
            metadata.Copyright =
                MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Copyright), metadata.Copyright);
            metadata.Description = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Description),
                metadata.Description);
            metadata.DiscNumber =
                MapResult(grokItem.Key, grokItem.Value, nameof(metadata.DiscNumber), metadata.DiscNumber);
            metadata.DiscTotal =
                MapResult(grokItem.Key, grokItem.Value, nameof(metadata.DiscTotal), metadata.DiscTotal);

            metadata.EncodedBy = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.EncodedBy), metadata.EncodedBy);
            metadata.EncoderSettings = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.EncoderSettings), metadata.EncoderSettings);
            metadata.EncodingTool = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.EncodingTool),
                metadata.EncodingTool);
            metadata.Genre = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Genre), metadata.Genre);
            metadata.Group = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Group), metadata.Group);
            metadata.ItunesCompilation = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.ItunesCompilation), metadata.ItunesCompilation);
            metadata.ItunesMediaType = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.ItunesMediaType),
                metadata.ItunesMediaType);
            metadata.ItunesPlayGap = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.ItunesPlayGap),
                metadata.ItunesPlayGap);
            metadata.LongDescription = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.LongDescription),
                metadata.LongDescription);
            //metadata.Lyrics = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Lyrics), metadata.Lyrics);

            metadata.Movement = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Movement), metadata.Movement);
            metadata.MovementName = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.MovementName),
                metadata.MovementName);
            metadata.Narrator = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Narrator), metadata.Narrator);

            metadata.OriginalAlbum = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.OriginalAlbum),
                metadata.OriginalAlbum);
            metadata.OriginalArtist = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.OriginalArtist),
                metadata.OriginalArtist);
            metadata.Popularity =
                MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Popularity), metadata.Popularity);
            metadata.Publisher =
                MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Publisher), metadata.Publisher);
            metadata.PublishingDate = MapDateResult(grokItem.Key, grokItem.Value, nameof(metadata.PublishingDate),
                metadata.PublishingDate);
            metadata.PurchaseDate = MapDateResult(grokItem.Key, grokItem.Value, nameof(metadata.PurchaseDate),
                metadata.PurchaseDate);
            metadata.RecordingDate = MapDateResult(grokItem.Key, grokItem.Value, nameof(metadata.RecordingDate),
                metadata.RecordingDate);
            metadata.SortTitle =
                MapResult(grokItem.Key, grokItem.Value, nameof(metadata.SortTitle), metadata.SortTitle);
            metadata.SortAlbum =
                MapResult(grokItem.Key, grokItem.Value, nameof(metadata.SortAlbum), metadata.SortAlbum);
            metadata.SortArtist =
                MapResult(grokItem.Key, grokItem.Value, nameof(metadata.SortArtist), metadata.SortArtist);
            metadata.SortAlbumArtist = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.SortAlbumArtist),
                metadata.SortAlbumArtist);
            metadata.Title = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.Title), metadata.Title);
            metadata.TrackNumber = MapResult(grokItem.Key, grokItem.Value, nameof(metadata.TrackNumber),
                metadata.TrackNumber);
            metadata.TrackTotal =
                MapResult(grokItem.Key, grokItem.Value, nameof(metadata.TrackTotal), metadata.TrackTotal);
        }
    }

    private static DateTime? MapDateResult(string grokItemKey, object grokItemValue, string propertyName,
        DateTime? defaultValue)
    {
        if (string.Equals(grokItemKey, propertyName, StringComparison.CurrentCultureIgnoreCase) &&
            TryParseDateTime(grokItemValue.ToString() ?? "", out var dateTime))
        {
            return dateTime;
        }

        return defaultValue;
    }

    private static bool TryParseDateTime(string dateTimeAsString, out DateTime dateTime)
    {
        if (dateTimeAsString.Length == 4 && dateTimeAsString.All(char.IsDigit))
        {
            dateTimeAsString += "-01-01";
        }

        return DateTime.TryParse(dateTimeAsString, out dateTime);
    }

    private T MapResult<T>(string grokItemKey, object grokItemValue, string propertyName, T defaultValue)
    {
        return string.Equals(grokItemKey, propertyName, StringComparison.CurrentCultureIgnoreCase)
            ? (T)grokItemValue
            : defaultValue;
    }
}