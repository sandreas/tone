using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using tone.Services;
using Serilog;
using tone.Common.Extensions.Stream;
using tone.Metadata;
using tone.Metadata.Formats;
using tone.Metadata.Taggers;

namespace tone.Commands;

[Command("tag")]
public class TagCommand : ICommand, IMetadata
{
    private readonly ILogger _logger;
    private readonly DirectoryLoaderService _dirLoader;
    private readonly ChptFmtNativeMetadataFormat _chapterFormat;

    [CommandOption("input", 'i')] public IReadOnlyList<string> Input { get; init; } = new List<string>();
    [CommandOption("path-pattern", 'p')] public IReadOnlyList<string> PathPattern { get; init; } = new List<string>();

    [CommandOption("assume-yes", 'y')] public bool AssumeYes { get; init; } = false;

    [CommandOption("dump")] public bool Dump { get; init; } = false;
    [CommandOption("short-dump")] public bool ShortDump { get; init; } = false;

    [CommandOption("include-extensions")]
    public IReadOnlyList<string> IncludeExtensions { get; init; } = new List<string>();

    [CommandOption("meta-remove")] public IReadOnlyList<string> Clear { get; init; } = new List<string>();

    [CommandOption("meta-title")] public string? Title { get; set; }
    [CommandOption("meta-artist")] public string? Artist { get; set; }
    [CommandOption("meta-composer")] public string? Composer { get; set; }
    [CommandOption("meta-comment")] public string? Comment { get; set; }
    [CommandOption("meta-genre")] public string? Genre { get; set; }
    [CommandOption("meta-album")] public string? Album { get; set; }
    [CommandOption("meta-original-album")] public string? OriginalAlbum { get; set; }
    [CommandOption("meta-copyright")] public string? Copyright { get; set; }
    [CommandOption("meta-description")] public string? Description { get; set; }
    [CommandOption("meta-publisher")] public string? Publisher { get; set; }
    [CommandOption("meta-album-artist")] public string? AlbumArtist { get; set; }
    [CommandOption("meta-conductor")] public string? Conductor { get; set; }

    [CommandOption("meta-recording-date")] public DateTime? RecordingDate { get; set; }
    [CommandOption("meta-track-number")] public int? TrackNumber { get; set; }
    [CommandOption("meta-track-total")] public int? TrackTotal { get; set; }
    [CommandOption("meta-disc-number")] public int? DiscNumber { get; set; }
    [CommandOption("meta-disc-total")] public int? DiscTotal { get; set; }
    [CommandOption("meta-popularity")] public float? Popularity { get; set; }

    [CommandOption("meta-chapters-table-description")]
    public string? ChaptersTableDescription { get; set; }


    [CommandOption("meta-group")] public string? Group { get; set; }

    [CommandOption("meta-sort-title")] public string? SortTitle { get; set; }

    [CommandOption("meta-sort-album")] public string? SortAlbum { get; set; }
    [CommandOption("meta-sort-artist")] public string? SortArtist { get; set; }

    [CommandOption("meta-sort-album-artist")]
    public string? SortAlbumArtist { get; set; }

    [CommandOption("meta-long-description")]
    public string? LongDescription { get; set; }

    [CommandOption("meta-encoding-tool")] public string? EncodingTool { get; set; }
    [CommandOption("meta-purchase-date")] public DateTime? PurchaseDate { get; set; }
    [CommandOption("meta-media-type")] public string? MediaType { get; set; }

    [CommandOption("meta-original-artist")]
    public string? OriginalArtist { get; set; }
    
    [CommandOption("meta-narrator")] public string? Narrator { get; set; }

    [CommandOption("meta-series-title")] public string? SeriesTitle { get; set; }

    [CommandOption("meta-series-part")] public string? SeriesPart { get; set; }

    [CommandOption("meta-publishing-date")]
    public DateTime? PublishingDate { get; set; }

    [CommandOption("meta-extra-fields")] public IReadOnlyList<string> ExtraFields { get; init; } = new List<string>();

    [CommandOption("meta-extra-fields-remove")]
    public IReadOnlyList<string> ExtraFieldsRemove { get; init; } = new List<string>();

    [CommandOption("meta-equate")] public IReadOnlyList<string> Equate { get; init; } = new List<string>();
    
    
    [CommandOption("auto-import-chapters")]
    public bool AutoImportChapters { get; init; } = false;

    [CommandOption("import-chapters-file")]
    public string ImportChaptersFile { get; init; } = "";


    // fulfil interface contract
    public string? Path => null;
    public TimeSpan TotalDuration => new();
    public IList<ChapterInfo>? Chapters { get; set; }
    public LyricsInfo? Lyrics { get; set; }
    public IList<PictureInfo>? EmbeddedPictures => null;

    public IDictionary<string, string>? AdditionalFields { get; set; }

    public TagCommand(ILogger logger, DirectoryLoaderService dirLoader, ChptFmtNativeMetadataFormat chapterFormat)
    {
        _logger = logger;
        _dirLoader = dirLoader;
        _chapterFormat = chapterFormat;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        _logger.Debug("tag command started");
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(Input, audioExtensions);
        if (Dump || ShortDump)
        {
            foreach (var file in inputFiles)
            {
                await DumpFileInfoAndMetadata(console, file);
            }

            return;
        }

        var inputFilesAsArray = inputFiles.ToArray();
        if (inputFilesAsArray.Length > 1 && !AssumeYes)
        {
            if (!await Confirm(console, $"Tagging {inputFilesAsArray.Length} files, continue?"))
            {
                await console.Output.WriteLineAsync("aborted");
                return;
            }
        }

        await DumpMetadata(console, this, ShortDump);

        var tagger = new TaggerComposite();
        tagger.Taggers.Add(new MetadataTagger(this));

        var defaultCustomPatterns = new[]
        {
            "NOTDIRSEP [^/\\\\]*"
        };
        tagger.Taggers.Add(new PathPatternTagger(PathPattern, defaultCustomPatterns));

        tagger.Taggers.Add(new ExtraFieldsTagger(ExtraFields));
        tagger.Taggers.Add(new ExtraFieldsRemoveTagger(ExtraFieldsRemove));
        if (AutoImportChapters || ImportChaptersFile != "")
        {
            tagger.Taggers.Add(new ChptFmtNativeTagger(_dirLoader.FileSystem, _chapterFormat,
                ImportChaptersFile));
        }
        
        tagger.Taggers.Add(new EquateTagger(Equate));
        tagger.Taggers.Add(new M4BFillUpTagger());

        var tasks = inputFilesAsArray.Select(file => Task.Run(async () =>
            {
                var track = new MetadataTrack(file);
                var result = await tagger.Update(track);
                if (!result)
                {
                    await console.Error.WriteLineAsync($"Could not update tags for file {file}: {result.Error}");
                    return;
                }

                // await DumpMetadata(console, track, ShortDump);

                if (!track.Save())
                {
                    await console.Error.WriteLineAsync($"Could not save tags for {file}");
                }
            }))
            .ToList();

        await Task.WhenAll(tasks);
    }

    private static async Task<bool> Confirm(IConsole console, string message, bool confirmIsDefault = false)
    {
        var confirmString = confirmIsDefault ? "[Y/n]" : "[y/N]";
        await console.Output.WriteAsync($"{message} {confirmString}");
        var answer = await console.Input.ReadLineAsync();
        return answer?.Trim().ToLower() != "y";
    }


    private async Task DumpFileInfoAndMetadata(IConsole console, IFileInfo file)
    {
        var track = new MetadataTrack(file.FullName);
        await DumpOptionalTag(console, "audio-format: ", track.AudioFormat);
        await DumpOptionalTag(console, "vbr: ", track.IsVBR);
        await DumpOptionalTag(console, "channels: ", track.ChannelsArrangement);
        await DumpOptionalTag(console, "bitrate: ", track.Bitrate);
        await DumpOptionalTag(console, "sample-rate: ", track.SampleRate);

        await DumpMetadata(console, track, ShortDump);
    }

    private async Task DumpMetadata(IConsole console, IMetadata track, bool shortDump)
    {
        if (track.EmbeddedPictures != null)
        {
            if (shortDump)
            {
                await DumpOptionalTag(console, "embedded-pictures: ", track.EmbeddedPictures?.Count);
            }
            else
            {
                await DumpEmbeddedPictures(console, "embedded-pictures: ", track.EmbeddedPictures);
            }
        }

        await DumpOptionalTag(console, "duration: ", track.TotalDuration);
        // await DumpOptionalTag(console, "codec family: ", track.CodecFamily);

        await console.Output.WriteLineAsync("");
        await console.Output.WriteLineAsync("");
        await DumpOptionalTag(console, "genre: ", track.Genre);
        await DumpOptionalTag(console, "artist: ", track.Artist);
        await DumpOptionalTag(console, "sort-artist: ", track.SortArtist);
        await DumpOptionalTag(console, "album-artist: ", track.AlbumArtist);
        await DumpOptionalTag(console, "sort-album-artist: ", track.SortAlbumArtist);
        await DumpOptionalTag(console, "original-artist: ", track.OriginalArtist);
        await DumpOptionalTag(console, "narrator: ", track.Narrator);
        await DumpOptionalTag(console, "composer: ", track.Composer);
        await DumpOptionalTag(console, "publisher: ", track.Publisher);
        await DumpOptionalTag(console, "album: ", track.Album);
        await DumpOptionalTag(console, "sort-album: ", track.SortAlbum);
        await DumpOptionalTag(console, "original-album: ", track.OriginalAlbum);
        await DumpOptionalTag(console, "title: ", track.Title);
        await DumpOptionalTag(console, "sort-title: ", track.SortTitle);
        await DumpOptionalTag(console, "series-title: ", track.SeriesTitle);
        await DumpOptionalTag(console, "series-part: ", track.SeriesPart);
        await DumpOptionalTag(console, "disc-number: ", track.DiscNumber, 0);
        await DumpOptionalTag(console, "disc-total: ", track.DiscTotal, 0);
        await DumpOptionalTag(console, "track-number: ", track.TrackNumber, 0);
        await DumpOptionalTag(console, "track-total: ", track.TrackTotal, 0);
        await DumpOptionalTag(console, "copyright: ", track.Copyright);
        await DumpOptionalTag(console, "publishing-date: ", track.PublishingDate, DateTime.MinValue);
        await DumpOptionalTag(console, "recording-date: ", track.RecordingDate, DateTime.MinValue);
        await DumpOptionalTag(console, "purchase-date: ", track.PurchaseDate, DateTime.MinValue);
        await DumpOptionalTag(console, "group: ", track.Group);
        await DumpOptionalTag(console, "encoding-tool: ", track.EncodingTool);
        await DumpOptionalTag(console, "media-type: ", track.MediaType);
        await DumpOptionalTag(console, "popularity: ", track.Popularity, 0);
        await DumpOptionalTag(console, "conductor: ", track.Conductor);
        if (track.AdditionalFields != null)
        {
            if (shortDump)
            {
                await DumpOptionalTag(console, "additional-fields: ", string.Join(", ", track.AdditionalFields.Keys));
            }
            else
            {
                await DumpAdditionalFields(console, "additional-fields: ", track.AdditionalFields);
            }
        }


        await DumpOptionalTag(console, "comment:\n", track.Comment);
        if (shortDump)
        {
            await DumpOptionalTag(console, "description (length): ", track.Description?.Length);
            await DumpOptionalTag(console, "long-description (length): ", track.LongDescription?.Length, 0);
            await DumpOptionalTag(console, "lyrics (length): ", track.Lyrics?.UnsynchronizedLyrics.Length, 0);
            await DumpOptionalTag(console, "chapters (count): ", track.Chapters?.Count);
        }
        else
        {
            await DumpOptionalTag(console, "description:\n", track.Description);
            await DumpOptionalTag(console, "long-description:\n", track.LongDescription);
            await DumpLyrics(console, "lyrics:", track.Lyrics);
            await DumpChapters(console, "chapters:\n", track);
        }
    }

    private async Task DumpChapters(IConsole console, string chapters, IMetadata track)
    {
        if (track.Chapters?.Count == 0)
        {
            return;
        }

        await using var chapterStream = new MemoryStream();
        if (await _chapterFormat.WriteAsync(track, chapterStream))
        {
            await DumpOptionalTag(console, "chapters-table-description:\n", track.ChaptersTableDescription);
            await DumpOptionalTag(console, chapters, chapterStream.StreamToString());
        }
    }

    private async Task DumpLyrics(IConsole console, string prefix, LyricsInfo? trackLyrics)
    {
        if (trackLyrics == null)
        {
            return;
        }

        List<string> parts = new();
        if (!string.IsNullOrWhiteSpace(trackLyrics.LanguageCode))
        {
            parts.Add("lyrics-language: " + trackLyrics.LanguageCode);
        }

        if (!string.IsNullOrWhiteSpace(trackLyrics.Description))
        {
            parts.Add("lyrics-description: " + trackLyrics.Description);
        }

        if (trackLyrics.SynchronizedLyrics.Count > 0)
        {
            parts.Add("lyrics (parts with timestamp): " + trackLyrics.SynchronizedLyrics.Count);
        }

        if (!string.IsNullOrWhiteSpace(trackLyrics.UnsynchronizedLyrics))
        {
            parts.Add("lyrics:\n" + trackLyrics.UnsynchronizedLyrics);
        }

        await DumpOptionalTag(console, prefix, string.Join("\n", parts));
    }

    private async Task DumpAdditionalFields(IConsole console, string additionalFields,
        IDictionary<string, string> trackAdditionalFields)
    {
        await console.Output.WriteLineAsync(additionalFields);
        foreach (var (key, value) in trackAdditionalFields)
        {
            await console.Output.WriteLineAsync(" - " + key + ": " + value);
        }
    }

    private async Task DumpEmbeddedPictures(IConsole console, string embeddedPictures,
        IList<PictureInfo?> trackEmbeddedPictures)
    {
        var embeddedPictureCount = trackEmbeddedPictures.Count(pic => pic != null);
        if (embeddedPictureCount > 0)
        {
            await console.Output.WriteLineAsync(embeddedPictures);
            foreach (var pic in trackEmbeddedPictures)
            {
                if (pic == null)
                {
                    continue;
                }

                await console.Output.WriteLineAsync("  " + pic.Position + ".) " + pic.MimeType + " (" +
                                                    pic.PictureData.Length + " bytes)");
            }
        }
    }

    private async Task DumpOptionalTag<T>(IConsole console, string prefix, T value, T? ignoreValue = default)
    {
        if (value == null || EqualityComparer<T>.Default.Equals(value, ignoreValue))
        {
            return;
        }

        if (value is string str && string.IsNullOrEmpty(str))
        {
            return;
        }

        await DumpTag(console, prefix, value);
    }

    private async Task DumpTag<T>(IConsole console, string prefix, T value)
    {
        switch (value)
        {
            case Format f:
                await console.Output.WriteLineAsync(prefix + f.Name);
                return;
            case TimeSpan duration:
            {
                var totalHoursAsString = Math.Floor(duration.TotalHours).ToString(CultureInfo.InvariantCulture)
                    .PadLeft(2, '0');
                await console.Output.WriteLineAsync(prefix + totalHoursAsString + duration.ToString(@"\:mm\:ss\.fff"));
                return;
            }
            case DateTime dateTime:
            {
                await console.Output.WriteLineAsync(prefix +
                                                    dateTime.ToString(CultureInfo.InvariantCulture)
                                                        .Replace(" 00:00:00", ""));
                return;
            }
            default:
                await console.Output.WriteLineAsync(prefix + value);
                break;
        }
    }
}