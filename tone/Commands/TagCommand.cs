using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using tone.Services;
using Serilog;
using tone.Metadata;
using tone.Metadata.Parsers;
using tone.Metadata.Taggers;

namespace tone.Commands;

[Command("tag")]
public class TagCommand : ICommand, IMetadata
{
    private readonly ILogger _logger;
    private readonly DirectoryLoaderService _dirLoader;
    private readonly ChptFmtNativeParser _chptFmtParser;

    [CommandOption("input", 'i')] public IReadOnlyList<string> Input { get; init; } = new List<string>();

    [CommandOption("assume-yes", 'y')] public bool AssumeYes { get; init; } = false;

    [CommandOption("dump")] public bool Dump { get; init; } = false;

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

    [CommandOption("meta-sort-name")] public string? SortName { get; set; }

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

    [CommandOption("meta-publishing-date")]
    public DateTime? PublishingDate { get; set; }

    [CommandOption("meta-extra-fields")]
    public IReadOnlyList<string> ExtraFields { get; init; } = new List<string>();

    [CommandOption("meta-extra-fields-remove")]
    public IReadOnlyList<string> ExtraFieldsRemove { get; init; } = new List<string>();

    // fulfil interface contract
    public string? Path => null;
    public TimeSpan TotalDuration => new();
    public IList<ChapterInfo>? Chapters { get; set; }
    public LyricsInfo? Lyrics { get; set; }
    public IList<PictureInfo>? EmbeddedPictures => null;

    public IDictionary<string, string>? AdditionalFields { get; set; }

    public TagCommand(ILogger logger, DirectoryLoaderService dirLoader, ChptFmtNativeParser chptFmtParser)
    {
        _logger = logger;
        _dirLoader = dirLoader;
        _chptFmtParser = chptFmtParser;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        _logger.Debug("tag command started");
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(Input, audioExtensions);
        if (Dump)
        {
            foreach (var file in inputFiles)
            {
                await DumpTags(console, file);
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

        
        
        var tagger = new TaggerComposite();
        tagger.Taggers.Add(new MetadataTagger(this));
        tagger.Taggers.Add(new ExtraFieldsTagger(ExtraFields));
        tagger.Taggers.Add(new ExtraFieldsRemoveTagger(ExtraFieldsRemove));
        tagger.Taggers.Add(new ChptFmtNativeTagger(_dirLoader.FileSystem, _chptFmtParser)); // CHPT_FMT_NATIVE
        
        var tasks = new List<Task>();
        foreach (var file in inputFilesAsArray)
        {
            tasks.Add(Task.Run(() =>
            {
                var track = new MetadataTrack(file);
                tagger.Update(track);
                if (!track.Save())
                {
                    console.Error.WriteLine($"Could not save tags for {file}");
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    private async Task<bool> Confirm(IConsole console, string message, bool confirmIsDefault = false)
    {
        var confirmString = confirmIsDefault ? "[Y/n]" : "[y/N]";
        await console.Output.WriteAsync($"{message} {confirmString}");
        var answer = await console.Input.ReadLineAsync();
        return answer?.Trim().ToLower() != "y";
    }


    private async Task DumpTags(IConsole console, IFileInfo file)
    {
        var track = new MetadataTrack(file.FullName);
        var properties = track.GetType().GetProperties();

        var skipProperties = new[] { "Year", "Duration" };
        // Special lists: Chapters, PictureTokens, MetadataFormats, EmbeddedPictures
        // Special props: Lyrics, AudioFormat, TechnicalInformation
        foreach (var prop in properties)
        {
            var name = prop.Name;
            if (skipProperties.Contains(name))
            {
                continue;
            }

            var propValue = prop.GetValue(track, null);

            if (name == "DurationMs")
            {
                name = "Duration";
                var duration = TimeSpan.FromMilliseconds((double)(propValue ?? 0));
                var totalHoursAsString = Math.Floor(duration.TotalHours).ToString(CultureInfo.InvariantCulture)
                    .PadLeft(2, '0');
                propValue = totalHoursAsString + duration.ToString(@"\:mm\:ss\.fff");
            }

            if (propValue is DateTime dateTime && dateTime == DateTime.MinValue)
            {
                propValue = "";
            }

            if (propValue is LyricsInfo lyrics)
            {
                propValue = lyrics.UnsynchronizedLyrics;
            }

            if (propValue is Format format)
            {
                propValue = format.Name + " - " + string.Join(", ", format.MimeList);
            }

            if (string.IsNullOrEmpty(propValue?.ToString()))
            {
                continue;
            }

            if (propValue is IList list)
            {
                propValue = $"<listing> (length: {list.Count})";
            }

            if (propValue is IDictionary<string, string> dict)
            {
                await console.Output.WriteLineAsync(name + ": ");
                foreach (var (key, value) in dict)
                {
                    await console.Output.WriteLineAsync(" - " + key + ": " + value);
                }

                continue;
            }

            await console.Output.WriteLineAsync(name + ": " + propValue);
        }
    }
}