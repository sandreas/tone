using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATL;
using OperationResult;
using Sandreas.AudioMetadata;
using static OperationResult.Helpers;

namespace tone.Metadata.Formats;

enum SectionType
{
    Default,
    Chapter,
    Stream
}

public class FfmetadataFormat : IMetadataFormat
{
    // https://ffmpeg.org/ffmpeg-formats.html#Description

    private const string FfmetadataHeader = ";FFMETADATA";
    private static readonly char[] CharsToEscape = { '=', ';', '#', '\\', '\n' };
    // TBPM=0,
    // TMED=CD,
    // language=eng,
    // TIPL=arranger,
    // artist-sort=12Stones,
    // TDOR=2002-04-23,
    // Script=Latn,
    // ASIN=B0000649OY
    // lyrics-XXX (where XXX is either XXX or language code, e.g. -eng)
    // disc
    //
    private static readonly List<(MetadataProperty property, Type type, string ffmetakey, string[]? ffmetaFallbackKeys)> KeyMapping = new()
    {
        (MetadataProperty.Album, typeof(string), "album",null),
        (MetadataProperty.AlbumArtist, typeof(string), "album_artist",null),
        (MetadataProperty.Artist, typeof(string), "artist",null),
        // (MetadataProperty.Bpm, typeof(string), "",null), // TBPM
        //(MetadataProperty.ChaptersTableDescription, typeof(string), "",null),
        (MetadataProperty.Composer, typeof(string), "composer",null),
        (MetadataProperty.Comment, typeof(string), "comment",null),
        // (MetadataProperty.Conductor, typeof(string), "",null),
        (MetadataProperty.Copyright, typeof(string), "copyright",null),
        (MetadataProperty.Description, typeof(string), "description",null),
        // (MetadataProperty.DiscNumber, typeof(string), "album",null), // disc = 1/1
        // (MetadataProperty.DiscTotal, typeof(string), "album",null),  // disc = 1/1
        (MetadataProperty.EncodedBy, typeof(string), "encoded_by",null),
        // (MetadataProperty.EncoderSettings, typeof(string), "album",null),
        (MetadataProperty.EncodingTool, typeof(string), "encoder",null),
        (MetadataProperty.Genre, typeof(string), "genre",null),
        (MetadataProperty.Group, typeof(string), "grouping",null),
        (MetadataProperty.ItunesCompilation, typeof(ItunesCompilation), "compilation",null),
        (MetadataProperty.ItunesMediaType, typeof(ItunesMediaType), "media_type",null),
        (MetadataProperty.ItunesPlayGap, typeof(ItunesGapless), "gapless_playback",null),
        (MetadataProperty.LongDescription, typeof(string), "synopsis",null),
        // (MetadataProperty.Lyrics, typeof(string), "lyrics-XXX",null),
        (MetadataProperty.Part, typeof(string), "PART",null),
        // (MetadataProperty.Movement, typeof(string), "",null),
        // (MetadataProperty.MovementName, typeof(string), "",null),
        // (MetadataProperty.Narrator, typeof(string), "album",null),
        // (MetadataProperty.OriginalAlbum, typeof(string), "album",null),
        // (MetadataProperty.OriginalArtist, typeof(string), "album",null),
        // (MetadataProperty.Popularity, typeof(string), "album",null),
        (MetadataProperty.Publisher, typeof(string), "publisher",null),
        // (MetadataProperty.PublishingDate, typeof(string), "album",null),
        (MetadataProperty.PurchaseDate, typeof(DateTime), "purchase_date",null),
        (MetadataProperty.RecordingDate, typeof(DateTime), "date",null),
        (MetadataProperty.SortTitle, typeof(string), "sort_title",new []{"title-sort"}),
        (MetadataProperty.SortAlbum, typeof(string), "sort_album",new []{"album-sort"}),
        (MetadataProperty.SortArtist, typeof(string), "sort_artist",new []{"artist-sort"}),
        // (MetadataProperty.SortAlbumArtist, typeof(string), "album",null),
        //(MetadataProperty.SortComposer, typeof(string), "album",null),
        //(MetadataProperty.Subtitle, typeof(string), "album",null),
        (MetadataProperty.Title, typeof(string), "title",null),
        // (MetadataProperty.TrackNumber, typeof(string), "album",null),
        // (MetadataProperty.TrackTotal, typeof(string), "album",null),
        // (MetadataProperty.Chapters, typeof(string), "album",null),
        // (MetadataProperty.EmbeddedPictures, typeof(string), "album",null),
        // (MetadataProperty.AdditionalFields, typeof(string), "album",null),
    };
    public async Task<Result<IMetadata, string>> ReadAsync(Stream input)
    {
        var metadata = new MetadataTrack();
        using var sr = new StreamReader(input);

        if (sr.Peek() < 0)
        {
            return Error("Peek failed");
        }

        var headerResult = await ReadHeaderAsync(sr);
        if (!headerResult)
        {
            return Error(headerResult.Error);
        }

        var currentSectionType = SectionType.Default;
        while (!sr.EndOfStream)
        {
            var properties = ReadSectionProperties(sr, currentSectionType, out var nextSectionType);
            switch (currentSectionType)
            {
                case SectionType.Chapter:
                    ParseChapterProperties(properties, metadata);
                    break;
                case SectionType.Default:
                case SectionType.Stream:
                default:
                    ParseMetadataProperties(properties, metadata);
                    break;
            }

            currentSectionType = nextSectionType;
        }

        return metadata;
    }

    private static void ParseMetadataProperties(Dictionary<string, string> properties, IMetadata metadata)
    {
        // var mappedKeys = new List<string>();
        foreach (var (property, type, ffmetakey, ffmetaFallbackKeys) in KeyMapping)
        {
            // mappedKeys.Add(ffmetakey);
            
            var orderedKeys = new List<string>();
            if (ffmetaFallbackKeys != null)
            {
                // mappedKeys.AddRange(ffmetaFallbackKeys);
                orderedKeys.AddRange(ffmetaFallbackKeys.Reverse());
            }
            orderedKeys.Add(ffmetakey);
            foreach (var propertyKey in orderedKeys.Where(properties.ContainsKey))
            {
                metadata.SetMetadataPropertyValue(property, properties[propertyKey], type);
            }
        }

        /*
        foreach (var (key, value) in properties.Where(kvp => !mappedKeys.Contains(kvp.Key)))
        {
            metadata.AdditionalFields[key] = value;
        }
        */
    }

        

    private static void ParseChapterProperties(Dictionary<string, string> properties, IMetadata metadata)
    {
        decimal timebase = -1;
        int start = -1;
        int end = -1;
        string title = "";
        foreach (var (key, value) in properties)
        {
            var normalizedKey = key.ToLowerInvariant();
            switch (normalizedKey)
            {
                case "timebase":
                    if (!TryParseTimeBaseInMilliseconds(value, out timebase))
                    {
                        return;
                    }
                    break;
                case "start":
                    if (!int.TryParse(value, out start))
                    {
                        return;
                    }
                    break;
                case "end":
                    if (!int.TryParse(value, out end))
                    {
                        return;
                    }
                    break;
                case "title":
                    title = value;
                    break;
            }
        }

        if (start == -1 || timebase == -1)
        {
            return;
        }

        var chapter = new ChapterInfo(Convert.ToUInt32(start * timebase), title);
        if (end != -1)
        {
            chapter.EndTime = Convert.ToUInt32(end * timebase);
        }
        metadata.Chapters.Add(chapter);
    }

    private static bool TryParseTimeBaseInMilliseconds(string value, out decimal timebase)
    {
        var parts = value.Split("/");
        if (parts.Length == 2 
            && decimal.TryParse(parts[0], out var meter) 
            && decimal.TryParse(parts[1], out var denominator) 
            && meter > 0 
            && denominator > 0)
        {
            timebase = meter / denominator * 1000;
            return true;
        }

        timebase = 1m;
        return false;
    }

    private Dictionary<string, string> ReadSectionProperties(TextReader sr, SectionType currentSectionType,
        out SectionType nextSectionType)
    {
        var properties = new Dictionary<string, string>();
        nextSectionType = SectionType.Default;
        while (sr.Peek() != -1)
        {
            var line = sr.ReadLine();
            if (line == null)
            {
                break;
            }

            if (line.StartsWith(";") || line.StartsWith("#"))
            {
                continue;
            }

            while (line.EndsWith("\\") && sr.Peek() != -1)
            {
                line = line.TrimEnd('\\') + "\n" + sr.ReadLine();
            }

            var trimmedLowerLine = line.Trim().ToLower();
            if (trimmedLowerLine == "[chapter]")
            {
                nextSectionType = SectionType.Chapter;
                break;
            }

            if (trimmedLowerLine == "[stream]")
            {
                nextSectionType = SectionType.Stream;
                break;
            }

            var (name, value) = ReadProperty(line);
            if (name != "")
            {
                properties[name] = value;
            }
        }

        return properties;
    }

    private (string, string) ReadProperty(string line)
    {
        var propertyNameBuilder = new StringBuilder();
        var propertyValueBuilder = new StringBuilder();
        var activeBuilder = propertyNameBuilder;
        var escaped = false;

        foreach (var c in line)
        {
            if (!escaped && c == '\\')
            {
                escaped = true;
                continue;
            }

            escaped = false;

            if (c == '=')
            {
                activeBuilder = propertyValueBuilder;
                continue;
            }

            activeBuilder.Append(c);
        }

        return (propertyNameBuilder.ToString(), propertyValueBuilder.ToString());
    }
/*
    private static string Unescape(string escapedString)
    {
        var builder = new StringBuilder();
        var escaped = false;
        foreach (var c in escapedString)
        {
            if (!escaped && c == '\\')
            {
                escaped = true;
                continue;
            }

            builder.Append(c);
            escaped = false;
        }

        return builder.ToString();
    }


    private IEnumerable<ChapterInfo> ReadChapters(StreamReader sr)
    {
        return new List<ChapterInfo>();
    }
*/

    private static async Task<Status<string>> ReadHeaderAsync(StreamReader sr)
    {
        while (!sr.EndOfStream)
        {
            var headerLine = await sr.ReadLineAsync();
            if (headerLine == null)
            {
                return Error("Could not find required header (null)");
            }

            if (headerLine.Trim() == "")
            {
                continue;
            }
            
            if (!headerLine.StartsWith(FfmetadataHeader))
            {
                return Error("Could not find required header " + FfmetadataHeader);
            }
            return Ok();
        }
        return Error("Could not find required header (EndOfStream)");
    }


    public async Task<Status<string>> WriteAsync(IMetadata input, Stream output)
    {
        // https://github.com/FFmpeg/FFmpeg/blob/a78f136f3fa039fd7ad664fd6e6e976f1448c68b/libavformat/mov.c
        var outputWriter = new StreamWriter(output);
        await outputWriter.WriteLineAsync(FfmetadataHeader);
        await WriteKeyValueAsync(outputWriter, "title", input.Title);
        await WriteKeyValueAsync(outputWriter, "artist", input.Artist);
        await WriteKeyValueAsync(outputWriter, "album", input.Album);
        await WriteKeyValueAsync(outputWriter, "composer", input.Composer);
        await WriteKeyValueAsync(outputWriter, "genre", input.Genre);
        await WriteKeyValueAsync(outputWriter, "date", input.RecordingDate, DateTime.MinValue); // 2022/03/31
        await WriteKeyValueAsync(outputWriter, "sort_name", input.SortTitle);
        await WriteKeyValueAsync(outputWriter, "sort_album", input.SortAlbum); // (album-sort)
        await WriteKeyValueAsync(outputWriter, "description", input.LongDescription);
        await WriteKeyValueAsync(outputWriter, "synopsis", input.LongDescription);
        await WriteKeyValueAsync(outputWriter, "copyright", input.Copyright);
        await WriteKeyValueAsync(outputWriter, "media_type", input.ItunesMediaType);
        await WriteKeyValueAsync(outputWriter, "purchase_date", input.PurchaseDate,
            DateTime.MinValue); // 2022/03/31 15:36:51
        await WriteKeyValueAsync(outputWriter, "encoder", input.EncodingTool);
        await WriteKeyValueAsync(outputWriter, "album_artist", input.AlbumArtist);
        // await WriteKeyValueAsync(outputWriter, "author", input.Author); 
        await WriteKeyValueAsync(outputWriter, "sort_artist", input.SortArtist);
        await WriteKeyValueAsync(outputWriter, "comment", input.Comment);

        await WriteKeyValueAsync(outputWriter, "compilation", input.ItunesCompilation);
        // await WriteKeyValueAsync(outputWriter, "creation_time", input.EncodingTime);
        await WriteKeyValueAsync(outputWriter, "encoded_by", input.EncodedBy);
        // await WriteKeyValueAsync(outputWriter, "episode_id", input.??);
        // await WriteKeyValueAsync(outputWriter, "episode_sort", input.??);
        // await WriteKeyValueAsync(outputWriter, "season_number", input.??);
        await WriteKeyValueAsync(outputWriter, "grouping", input.Group);
        //await WriteKeyValueAsync(outputWriter, "hd_video", input.??);
        // await WriteKeyValueAsync(outputWriter, "language", input.??);
        // await WriteKeyValueAsync(outputWriter, "lyrics", input.Lyrics);
        // await WriteKeyValueAsync(outputWriter, "network", input.??);
        await WriteKeyValueAsync(outputWriter, "publisher", input.Publisher);
        // await WriteKeyValueAsync(outputWriter, "producer", input.??);
        // await WriteKeyValueAsync(outputWriter, "performer", input.??);
        // await WriteKeyValueAsync(outputWriter, "director", input.??);
        // await WriteKeyValueAsync(outputWriter, "show", input.??);
        await WriteKeyValueAsync(outputWriter, "subtitle", input.Subtitle);
        
        // todo: slash notation (track: 1/10)
        await WriteKeyValueAsync(outputWriter, "track", input.TrackNumber, 0);
        await WriteKeyValueAsync(outputWriter, "disc", input.DiscNumber, 0);
        // await WriteKeyValueAsync(outputWriter, "rating", input.??); // Rating (0 = none, 1 = clean, 2 = explicit) 
        // await WriteKeyValueAsync(outputWriter, "location", input.?);
        // await WriteKeyValueAsync(outputWriter, "keywords", input.?);
        // await WriteKeyValueAsync(outputWriter, "URL", input.?);
        // await WriteKeyValueAsync(outputWriter, "podcast", input.?);
        // await WriteKeyValueAsync(outputWriter, "category", input.?);
        // await WriteKeyValueAsync(outputWriter, "episode_uid", input.?);


        foreach (var chapter in input.Chapters)
        {
            await outputWriter.WriteLineAsync("[CHAPTER]");
            await WriteKeyValueAsync(outputWriter, "TIMEBASE", "1/1000");
            await WriteKeyValueAsync(outputWriter, "START", chapter.StartTime, uint.MaxValue);
            await WriteKeyValueAsync(outputWriter, "END", chapter.EndTime);
            await WriteKeyValueAsync(outputWriter, "title", chapter.Title);
        }

        await outputWriter.FlushAsync();
        return Ok();
    }

    private static async Task WriteKeyValueAsync<T>(TextWriter outputWriter, string key, T? value,
        T? ignoreValue = default)
    {
        if (value == null)
        {
            return;
        }

        if (EqualityComparer<T>.Default.Equals(value, ignoreValue))
        {
            return;
        }

        var stringValue = value switch
        {
            DateTime d => d.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss").Replace(" 00:00:00", ""),
            Enum e => ((int)(object)e).ToString(),
            _ => value.ToString() ?? ""
        };
        if (stringValue != "")
        {
            await outputWriter.WriteLineAsync(Escape(key) + "=" + Escape(stringValue));
        }
    }

    private static string Escape(string str)
    {
        var builder = new StringBuilder();

        foreach (var c in str)
        {
            if (CharsToEscape.Contains(c))
            {
                builder.Append('\\');
            }

            builder.Append(c);
        }

        return builder.ToString();
    }
}