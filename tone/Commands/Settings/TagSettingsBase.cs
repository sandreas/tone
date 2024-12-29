using System;
using System.Collections.Generic;
using System.Linq;
using ATL;
using Sandreas.AudioMetadata;
using Spectre.Console.Cli;
using tone.Commands.Settings.Interfaces;
using tone.Commands.Settings.SettingValues;
using tone.Services;

namespace tone.Commands.Settings;

public class TagSettingsBase : CommandSettingsBase, 
    IMetadata, 
    ICoverTaggerSettings, 
    IToneJsonTaggerSettings, 
    IPathPatternSettings, 
    IChptFmtNativeTaggerSettings, 
    IFfmetadataTaggerSettings,
    IRemoveTaggerSettings, 
    ITaggerOrderSettings, 
    // IIdTaggerSettings, // IScriptSettings already implements IIdTaggerSettings 
    IScriptSettings,
    IPrependMovementToDescriptionTaggerSettings
{
    [CommandOption("--assume-yes|-y")] public bool AssumeYes { get; init; } = false;
    [CommandOption("--dry-run")] public bool DryRun { get; init; } = false;

    [CommandOption("--format")] public SerializerFormat Format { get; set; } = SerializerFormat.Default;
    [CommandOption("--id")] public string Id { get; set; } = "";

    [CommandOption("--taggers")]
    public string[] Taggers { get; set; } = Array.Empty<string>();
    
    [CommandOption("--script")]
    public string[] Scripts { get; set; } = Array.Empty<string>();
    
    [CommandOption("--script-tagger-parameter")]
    public string[] ScriptTaggerParameters { get; set; } = Array.Empty<string>();
    
    
    [CommandOption("--prepend-movement-to-description")] public bool PrependMovementToDescription { get; init; } = false;

    
    [CommandOption("--meta-artist")] public string? Artist { get; set; }
    [CommandOption("--meta-album")] public string? Album { get; set; }
    [CommandOption("--meta-album-artist")] public string? AlbumArtist { get; set; }
    [CommandOption("--meta-bpm")] public int? Bpm { get; set; }

    [CommandOption("--meta-chapters-table-description")]
    public string? ChaptersTableDescription { get; set; }

    [CommandOption("--meta-comment")] public string? Comment { get; set; }

    [CommandOption("--meta-composer")] public string? Composer { get; set; }
    [CommandOption("--meta-conductor")] public string? Conductor { get; set; }
    [CommandOption("--meta-copyright")] public string? Copyright { get; set; }
    [CommandOption("--meta-description")] public string? Description { get; set; }
    [CommandOption("--meta-disc-number")] public int? DiscNumber { get; set; }
    [CommandOption("--meta-disc-total")] public int? DiscTotal { get; set; }

    [CommandOption("--meta-encoded-by")] public string? EncodedBy { get; set; }

    [CommandOption("--meta-encoder-settings")]
    public string? EncoderSettings { get; set; }

    [CommandOption("--meta-encoding-tool")]
    public string? EncodingTool { get; set; }

    [CommandOption("--meta-genre")] public string? Genre { get; set; }
    [CommandOption("--meta-group")] public string? Group { get; set; }

    [CommandOption("--meta-itunes-compilation")]
    public ItunesCompilation? ItunesCompilation { get; set; }

    [CommandOption("--meta-itunes-media-type")]
    public ItunesMediaType? ItunesMediaType { get; set; }


    [CommandOption("--meta-itunes-play-gap")]
    public ItunesPlayGap? ItunesPlayGap { get; set; }


    [CommandOption("--meta-long-description")]
    public string? LongDescription { get; set; }

    [CommandOption("--meta-part")] public string? Part { get; set; }
    [CommandOption("--meta-movement")] public string? Movement { get; set; }

    [CommandOption("--meta-movement-name")]
    public string? MovementName { get; set; }

    [CommandOption("--meta-narrator")] public string? Narrator { get; set; }

    [CommandOption("--meta-original-album")]
    public string? OriginalAlbum { get; set; }

    [CommandOption("--meta-original-artist")]
    public string? OriginalArtist { get; set; }

    [CommandOption("--meta-popularity")] public float? Popularity { get; set; }
    [CommandOption("--meta-publisher")] public string? Publisher { get; set; }

    [CommandOption("--meta-publishing-date")]
    public DateTime? PublishingDate { get; set; }

    [CommandOption("--meta-purchase-date")]
    public DateTime? PurchaseDate { get; set; }

    [CommandOption("--meta-recording-date")]
    public DateTime? RecordingDate { get; set; }

    [CommandOption("--meta-sort-album")] public string? SortAlbum { get; set; }

    [CommandOption("--meta-sort-album-artist")]
    public string? SortAlbumArtist { get; set; }

    [CommandOption("--meta-sort-artist")] public string? SortArtist { get; set; }

    [CommandOption("--meta-sort-composer")]
    public string? SortComposer { get; set; }

    [CommandOption("--meta-sort-title")] public string? SortTitle { get; set; }
    [CommandOption("--meta-subtitle")] public string? Subtitle { get; set; }
    [CommandOption("--meta-title")] public string? Title { get; set; }
    [CommandOption("--meta-track-number")] public int? TrackNumber { get; set; }
    [CommandOption("--meta-track-total")] public int? TrackTotal { get; set; }

    [CommandOption("--meta-additional-field")]
    public IDictionary<string, string> AdditionalFields { get; set; } = new Dictionary<string, string>();

    [CommandOption("--auto-import")]
    public AutoImportValue[] AutoImport { get; init; } = Array.Empty<AutoImportValue>();
    
    [CommandOption("--meta-chapters-file")] public string ImportChaptersFile { get; init; } = "";
    [CommandOption("--meta-ffmetadata-file")] public string ImportFfmetadataFile { get; init; } = "";

    [CommandOption("--meta-cover-file")] public string[] Covers { get; set; } = Array.Empty<string>();    
    [CommandOption("--meta-tone-json-file")] public string[] ToneJsonFiles { get; set; } = Array.Empty<string>();    

    [CommandOption("--path-pattern|-p")] public string[] PathPattern { get; set; } = Array.Empty<string>();

    [CommandOption("--path-pattern-extension")]
    public string[] PathPatternExtension { get; set; } = Array.Empty<string>();
    
    [CommandOption("--meta-equate")] public string[] Equate { get; init; } = Array.Empty<string>();

    [CommandOption("--meta-remove-additional-field")]
    public string[] RemoveAdditionalFields { get; init; } = Array.Empty<string>();

    [CommandOption("--meta-remove-property")]
    public MetadataProperty[] Remove { get; init; } = Array.Empty<MetadataProperty>();

    // fulfil interface contract
    public string? Path => null;
    public string? BasePath => null;

    public TimeSpan TotalDuration => new();
    public IList<ChapterInfo> Chapters { get; set; } = new List<ChapterInfo>();
    public LyricsInfo? Lyrics { get; set; }
    public IList<PictureInfo> EmbeddedPictures => new List<PictureInfo>();

    public IDictionary<string, string> MappedAdditionalFields { get; } = new Dictionary<string, string>();

    // tagger specific    
    public bool AutoImportCovers => AutoImport.Contains(AutoImportValue.Covers);
    public bool AutoImportChapters => AutoImport.Contains(AutoImportValue.Chapters);
    public bool AutoImportFfmetadata => AutoImport.Contains(AutoImportValue.Ffmetadata);
    public bool AutoImportToneJson => AutoImport.Contains(AutoImportValue.ToneJson);

    /*
    public override ValidationResult Validate()
    {
        return Name.Length < 2
            ? ValidationResult.Error("Names must be at least two characters long")
            : ValidationResult.Success();
    }
    */
}