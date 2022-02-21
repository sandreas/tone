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

namespace tone.Commands;

[Command("tag")]
public class TagCommand : ICommand, IMetadata
{
    private readonly ILogger _logger;
    private readonly DirectoryLoaderService _dirLoader;

    [CommandOption("input", 'i')] public IReadOnlyList<string> Input { get; init; } = new List<string>();

    [CommandOption("assume-yes", 'y')] public bool AssumeYes { get; init; } = false;

    [CommandOption("dump")] public bool Dump { get; init; } = false;

    [CommandOption("include-extensions")]
    public IReadOnlyList<string> IncludeExtensions { get; init; } = new List<string>();
    
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

    /*
    [CommandOption("meta-date")] public DateTime? Date { get; set; }
    [CommandOption("meta-track-number")]public int? TrackNumber { get; set; }
    [CommandOption("meta-track-total")]public int? TrackTotal { get; set; }
    [CommandOption("meta-disc-number")]public int? DiscNumber { get; set; }
    [CommandOption("meta-disc-total")]public int? DiscTotal { get; set; }
    [CommandOption("meta-popularity")]public float? Popularity { get; set; }
*/
    [CommandOption("meta-original-artist")]
    public string? OriginalArtist { get; set; }

    [CommandOption("meta-publishing-date")]
    public DateTime? PublishingDate { get; set; }

    public TagCommand(ILogger logger, DirectoryLoaderService dirLoader)
    {
        _logger = logger;
        _dirLoader = dirLoader;
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

        var tasks = new List<Task>();
        foreach (var file in inputFilesAsArray)
        {
            var track = new MetadataTrack(file);
            CopyMetadata(this, track);
            tasks.Add(Task.Run(() =>
            {
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

    private void CopyMetadata(IMetadata source, IMetadata destination)
    {
        destination.Album = source.Album ?? destination.Album;
        destination.Title = source.Title ?? destination.Title;
        destination.Artist = source.Artist ?? destination.Artist;
        destination.Composer = source.Composer ?? destination.Composer;
        destination.Comment = source.Comment ?? destination.Comment;
        destination.Genre = source.Genre ?? destination.Genre;
        destination.Album = source.Album ?? destination.Album;
        destination.OriginalAlbum = source.OriginalAlbum ?? destination.OriginalAlbum;
        destination.Copyright = source.Copyright ?? destination.Copyright;
        destination.Description = source.Description ?? destination.Description;
        destination.Publisher = source.Publisher ?? destination.Publisher;
        destination.AlbumArtist = source.AlbumArtist ?? destination.AlbumArtist;
        destination.Conductor = source.Conductor ?? destination.Conductor;
        destination.Group = source.Group ?? destination.Group;
        destination.SortName = source.SortName ?? destination.SortName;
        destination.SortAlbum = source.SortAlbum ?? destination.SortAlbum;
        destination.SortArtist = source.SortArtist ?? destination.SortArtist;
        destination.SortAlbumArtist = source.SortAlbumArtist ?? destination.SortAlbumArtist;
        destination.LongDescription = source.LongDescription ?? destination.LongDescription;
        destination.EncodingTool = source.EncodingTool ?? destination.EncodingTool;
        destination.PurchaseDate = source.PurchaseDate ?? destination.PurchaseDate;
        destination.MediaType = source.MediaType ?? destination.MediaType;
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