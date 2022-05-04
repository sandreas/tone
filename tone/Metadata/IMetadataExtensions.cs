using System;
using System.Collections.Generic;
using System.Linq;
using ATL;

namespace tone.Metadata;

public static class MetadataExtensions
{
    public static Type? GetMetadataPropertyType(this IMetadata metadata, MetadataProperty property) => property switch
    {
        MetadataProperty.Album => typeof(string),
        MetadataProperty.AlbumArtist => typeof(string),
        MetadataProperty.Artist => typeof(string),
        MetadataProperty.Bpm => typeof(int),
        MetadataProperty.ChaptersTableDescription => typeof(string),
        MetadataProperty.Composer => typeof(string),
        MetadataProperty.Comment => typeof(string),
        MetadataProperty.Conductor => typeof(string),
        MetadataProperty.Copyright => typeof(string),
        MetadataProperty.Description => typeof(string),
        MetadataProperty.DiscNumber => typeof(int),
        MetadataProperty.DiscTotal => typeof(int),
        MetadataProperty.EncodedBy => typeof(string),
        MetadataProperty.EncoderSettings => typeof(string),
        MetadataProperty.EncodingTool => typeof(string),
        MetadataProperty.Genre => typeof(string),
        MetadataProperty.Group => typeof(string),
        MetadataProperty.ItunesCompilation => typeof(ItunesCompilation),
        MetadataProperty.ItunesMediaType => typeof(ItunesMediaType),
        MetadataProperty.ItunesPlayGap => typeof(ItunesPlayGap),
        MetadataProperty.LongDescription => typeof(string),
        MetadataProperty.Lyrics => typeof(LyricsInfo),
        MetadataProperty.Movement => typeof(string),
        MetadataProperty.MovementName => typeof(string),
        MetadataProperty.Narrator => typeof(string),
        MetadataProperty.OriginalAlbum => typeof(string),
        MetadataProperty.OriginalArtist => typeof(string),
        MetadataProperty.Popularity => typeof(int),
        MetadataProperty.Publisher => typeof(string),
        MetadataProperty.PublishingDate => typeof(DateTime),
        MetadataProperty.PurchaseDate => typeof(DateTime),
        MetadataProperty.RecordingDate => typeof(DateTime),
        MetadataProperty.SortTitle => typeof(string),
        MetadataProperty.SortAlbum => typeof(string),
        MetadataProperty.SortArtist => typeof(string),
        MetadataProperty.SortAlbumArtist => typeof(string),
        MetadataProperty.SortComposer => typeof(string),
        MetadataProperty.Subtitle => typeof(string),
        MetadataProperty.Title => typeof(string),
        MetadataProperty.TrackNumber => typeof(int),
        MetadataProperty.TrackTotal => typeof(int),
        MetadataProperty.Chapters => typeof(IList<ChapterInfo>),
        MetadataProperty.EmbeddedPictures => typeof(IList<PictureInfo>),
        MetadataProperty.AdditionalFields => typeof(IDictionary<string, string>),
        _ => null
    };

    public static object? GetMetadataPropertyValue(this IMetadata metadata, MetadataProperty property) =>
        property switch
        {
            MetadataProperty.Album => metadata.Album,
            MetadataProperty.AlbumArtist => metadata.AlbumArtist,
            MetadataProperty.Artist => metadata.Artist,
            MetadataProperty.Bpm => metadata.Bpm,
            MetadataProperty.ChaptersTableDescription => metadata.ChaptersTableDescription,
            MetadataProperty.Composer => metadata.Composer,
            MetadataProperty.Comment => metadata.Comment,
            MetadataProperty.Conductor => metadata.Conductor,
            MetadataProperty.Copyright => metadata.Copyright,
            MetadataProperty.Description => metadata.Description,
            MetadataProperty.DiscNumber => metadata.DiscNumber,
            MetadataProperty.DiscTotal => metadata.DiscTotal,
            MetadataProperty.EncodedBy => metadata.EncodedBy,
            MetadataProperty.EncoderSettings => metadata.EncoderSettings,
            MetadataProperty.EncodingTool => metadata.EncodingTool,
            MetadataProperty.Genre => metadata.Genre,
            MetadataProperty.Group => metadata.Group,
            MetadataProperty.ItunesCompilation => metadata.ItunesCompilation,
            MetadataProperty.ItunesMediaType => metadata.ItunesMediaType,
            MetadataProperty.ItunesPlayGap => metadata.ItunesPlayGap,
            MetadataProperty.LongDescription => metadata.LongDescription,
            MetadataProperty.Lyrics => metadata.Lyrics,
            MetadataProperty.Movement => metadata.Movement,
            MetadataProperty.MovementName => metadata.MovementName,
            MetadataProperty.Narrator => metadata.Narrator,
            MetadataProperty.OriginalAlbum => metadata.OriginalAlbum,
            MetadataProperty.OriginalArtist => metadata.OriginalArtist,
            MetadataProperty.Popularity => metadata.Popularity,
            MetadataProperty.Publisher => metadata.Publisher,
            MetadataProperty.PublishingDate => metadata.PublishingDate,
            MetadataProperty.PurchaseDate => metadata.PurchaseDate,
            MetadataProperty.RecordingDate => metadata.RecordingDate,
            MetadataProperty.SortTitle => metadata.SortTitle,
            MetadataProperty.SortAlbum => metadata.SortAlbum,
            MetadataProperty.SortArtist => metadata.SortArtist,
            MetadataProperty.SortAlbumArtist => metadata.SortAlbumArtist,
            MetadataProperty.SortComposer => metadata.SortComposer,
            MetadataProperty.Subtitle => metadata.Subtitle,
            MetadataProperty.Title => metadata.Title,
            MetadataProperty.TrackNumber => metadata.TrackNumber,
            MetadataProperty.TrackTotal => metadata.TrackTotal,
            MetadataProperty.Chapters => metadata.Chapters,
            MetadataProperty.EmbeddedPictures => metadata.EmbeddedPictures,
            MetadataProperty.AdditionalFields => metadata.AdditionalFields,
            _ => null
        };

    public static void SetMetadataPropertyValue(this IMetadata metadata, MetadataProperty property, string? value,
        Type type)
    {
        var valueToSet = value == null ? null : ConvertStringToType(value, type);
        metadata.SetMetadataPropertyValue(property, valueToSet);
    }
    private static object? ConvertStringToType(string grokItemValue, Type type) => type switch
    {
        var t when t == typeof(string) => grokItemValue,
        var t when t == typeof(DateTime) => TryParseDateTime(grokItemValue, out var dateTime) ? dateTime : null,
        var t when t == typeof(int) => int.TryParse(grokItemValue, out var i) ? i : null,
        var t when t == typeof(ItunesCompilation) => Enum.TryParse(grokItemValue, out ItunesCompilation i) ? i : null,
        var t when t == typeof(ItunesMediaType) => Enum.TryParse(grokItemValue, out ItunesMediaType i) ? i : null,
        var t when t == typeof(ItunesPlayGap) => Enum.TryParse(grokItemValue, out ItunesPlayGap i) ? i : null,
        var t when t == typeof(LyricsInfo) => string.IsNullOrWhiteSpace(grokItemValue) ? null : new LyricsInfo {ContentType = LyricsInfo.LyricsType.LYRICS, UnsynchronizedLyrics = grokItemValue},
        var t when t == typeof(IList<ChapterInfo>) => null,
        var t when t == typeof(IList<PictureInfo>) => null, // todo: maybe check, if this is a cover file?
        var t when t == typeof(IDictionary<string, string>) => null, // todo: maybe parse json?
        _ => null
    };
    
    /*
    public static object ConvertStringToType<T>() where T:struct => default(T) switch {
        byte => "byte",
        sbyte => "sbyte"
        _ => "Not a byte or sbyte"
    };
    */
    private static bool TryParseDateTime(string dateTimeAsString, out DateTime dateTime)
    {
        if (dateTimeAsString.Length == 4 && dateTimeAsString.All(char.IsDigit))
        {
            dateTimeAsString += "-01-01";
        }

        return DateTime.TryParse(dateTimeAsString, out dateTime);
    }
    public static void SetMetadataPropertyValue(this IMetadata metadata, MetadataProperty property, object? value)
    {
        switch (property)
        {
            case MetadataProperty.Album:
                metadata.Album = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.AlbumArtist:
                metadata.AlbumArtist = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Artist:
                metadata.Artist = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Bpm:
                metadata.Bpm = ObjectAsType<int?>(value);
                break;
            case MetadataProperty.ChaptersTableDescription:
                metadata.ChaptersTableDescription = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Composer:
                metadata.Composer = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Comment:
                metadata.Comment = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Conductor:
                metadata.Conductor = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Copyright:
                metadata.Copyright = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Description:
                metadata.Description = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.DiscNumber:
                metadata.DiscNumber = ObjectAsType<int?>(value);
                break;
            case MetadataProperty.DiscTotal:
                metadata.DiscTotal = ObjectAsType<int?>(value);
                break;
            case MetadataProperty.EncodedBy:
                metadata.EncodedBy = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.EncoderSettings:
                metadata.EncoderSettings = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.EncodingTool:
                metadata.EncodingTool = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Genre:
                metadata.Genre = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Group:
                metadata.Group = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.ItunesCompilation:
                metadata.ItunesCompilation = ObjectAsType<ItunesCompilation?>(value);
                break;
            case MetadataProperty.ItunesMediaType:
                metadata.ItunesMediaType = ObjectAsType<ItunesMediaType?>(value);
                break;
            case MetadataProperty.ItunesPlayGap:
                metadata.ItunesPlayGap = ObjectAsType<ItunesPlayGap?>(value);
                break;
            case MetadataProperty.LongDescription:
                metadata.LongDescription = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Lyrics:
                metadata.Lyrics = ObjectAsType<LyricsInfo>(value);
                break;
            case MetadataProperty.Movement:
                metadata.Movement = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.MovementName:
                metadata.MovementName = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Narrator:
                metadata.Narrator = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.OriginalAlbum:
                metadata.OriginalAlbum = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.OriginalArtist:
                metadata.OriginalArtist = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Popularity:
                metadata.Popularity = ObjectAsType<float?>(value);
                break;
            case MetadataProperty.Publisher:
                metadata.Publisher = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.PublishingDate:
                metadata.PublishingDate = ObjectAsType<DateTime?>(value);
                break;
            case MetadataProperty.PurchaseDate:
                metadata.PurchaseDate = ObjectAsType<DateTime?>(value);
                break;
            case MetadataProperty.RecordingDate:
                metadata.RecordingDate = ObjectAsType<DateTime?>(value);
                break;
            case MetadataProperty.SortTitle:
                metadata.SortTitle = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.SortAlbum:
                metadata.SortAlbum = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.SortArtist:
                metadata.SortArtist = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.SortAlbumArtist:
                metadata.SortAlbumArtist = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.SortComposer:
                metadata.SortComposer = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Subtitle:
                metadata.Subtitle = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.Title:
                metadata.Title = ObjectAsType<string?>(value);
                break;
            case MetadataProperty.TrackNumber:
                metadata.TrackNumber = ObjectAsType<int?>(value);
                break;
            case MetadataProperty.TrackTotal:
                metadata.TrackTotal = ObjectAsType<int?>(value);
                break;
            case MetadataProperty.Chapters:
                var chapters = ObjectAsType<IList<ChapterInfo>?>(value) ?? new List<ChapterInfo>();
                metadata.Chapters.Clear();
                foreach (var chapter in chapters)
                {
                    metadata.Chapters.Add(chapter);
                }

                break;
            case MetadataProperty.EmbeddedPictures:
                var pictures = ObjectAsType<IList<PictureInfo>?>(value) ?? new List<PictureInfo>();
                metadata.EmbeddedPictures.Clear();
                foreach (var picture in pictures)
                {
                    metadata.EmbeddedPictures.Add(picture);
                }

                break;
            case MetadataProperty.AdditionalFields:
                var additionalFields = ObjectAsType<IDictionary<string, string>?>(value) ??
                                       new Dictionary<string, string>();
                foreach (var kvp in metadata.MappedAdditionalFields)
                {
                    additionalFields[kvp.Key] = kvp.Value;
                }
                metadata.AdditionalFields.Clear();
                foreach (var kvp in additionalFields)
                {
                        metadata.AdditionalFields.Add(kvp);                        
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(property), property, null);
        }
    }

    private static T? ObjectAsType<T>(object? value)
    {
        if (value is T v)
        {
            return v;
        }

        return default;
    }

    public static void MergeProperties(this IMetadata metadata, IMetadata source)
    {
        var properties = Enum.GetValues<MetadataProperty>();
        foreach (var property in properties)
        {
            var destinationPropertyValue = metadata.GetMetadataPropertyValue(property);
            if (IsConsideredEmpty(destinationPropertyValue))
            {
                metadata.SetMetadataPropertyValue(property, source.GetMetadataPropertyValue(property));
            }
        }
    }

    public static void OverwriteProperties(this IMetadata metadata, IMetadata source)
    {
        var properties = Enum.GetValues<MetadataProperty>();
        foreach (var property in properties)
        {
            var newValue = source.GetMetadataPropertyValue(property);
            if (IsConsideredEmpty(newValue))
            {
                continue;
            }
            metadata.SetMetadataPropertyValue(property, newValue);
        }
    }

    private static bool IsConsideredEmpty(object? value) => value switch
    {
        null => true,
        string s when string.IsNullOrEmpty(s) => true,
        DateTime d when d == DateTime.MinValue => true,
        IList<ChapterInfo> { Count: 0 } 
            or IList<PictureInfo> { Count: 0 }
            or IDictionary<string, string> { Count: 0 } => true,
        _ => false
    };

    public static void ClearProperties(this IMetadata metadata, IEnumerable<MetadataProperty>? propertiesToKeep = null)
    {
        var properties = Enum.GetValues<MetadataProperty>();
        var propertiesToKeepArray = propertiesToKeep?.ToArray() ?? Array.Empty<MetadataProperty>();
        foreach (var property in properties)
        {
            if (propertiesToKeepArray.Length == 0 || !propertiesToKeepArray.Contains(property))
            {
                metadata.SetMetadataPropertyValue(property, null);
            }
        }
    }
}