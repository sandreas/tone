using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using tone.Services;
using Serilog;
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
    private readonly GrokPatternService _grok;

    [CommandOption("input", 'i')] public IReadOnlyList<string> Input { get; init; } = new List<string>();
    [CommandOption("include-extensions")]
    public IReadOnlyList<string> IncludeExtensions { get; init; } = new List<string>();

    [CommandOption("assume-yes", 'y')] public bool AssumeYes { get; init; } = false;
    [CommandOption("path-pattern", 'p')] public IReadOnlyList<string> PathPattern { get; init; } = new List<string>();
    [CommandOption("path-pattern-extension")] public IReadOnlyList<string> PathPatternExtension { get; init; } = new List<string>();
    

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

    public TagCommand(ILogger logger, DirectoryLoaderService dirLoader, ChptFmtNativeMetadataFormat chapterFormat, GrokPatternService grok)
    {
        _logger = logger;
        _dirLoader = dirLoader;
        _chapterFormat = chapterFormat;
        _grok = grok;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        _logger.Debug("tag command started");
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(Input, audioExtensions);


        var inputFilesAsArray = inputFiles.ToArray();
        if (inputFilesAsArray.Length > 1 && !AssumeYes)
        {
            if (!await Confirm(console, $"Tagging {inputFilesAsArray.Length} files, continue?"))
            {
                await console.Output.WriteLineAsync("aborted");
                return;
            }
        }

        // await DumpMetadata(console, this, ShortDump);

        var tagger = new TaggerComposite();
        tagger.Taggers.Add(new MetadataTagger(this));

        var customPatterns = PathPatternExtension.Concat(new[]
        {
            "NOTDIRSEP [^/\\\\]*"
        });

        var grokDefinitions = await _grok.BuildAsync(PathPattern, customPatterns);
        if (grokDefinitions)
        {
            tagger.Taggers.Add(new PathPatternTagger(grokDefinitions.Value));            
        }
        else
        {
            await console.Error.WriteAsync("Could not parse `--path-pattern`: " + grokDefinitions.Error);
            return;
        }

        
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
}