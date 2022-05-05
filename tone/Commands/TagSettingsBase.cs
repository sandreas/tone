using System;
using System.Collections.Generic;
using System.ComponentModel;
using ATL;
using Spectre.Console.Cli;
using tone.Metadata;

namespace tone.Commands;

public class TagSettingsBase : CommandSettingsBase, IMetadata
{
    [CommandOption("--meta-additional-fields-remove")]
    public string[] RemoveAdditionalFields { get; init; } = Array.Empty<string>();

    [CommandOption("--meta-remove")] public string[] Remove { get; init; } = Array.Empty<string>();
    [CommandOption("--meta-equate")] public string[] Equate { get; init; } = Array.Empty<string>();

    [CommandOption("--auto-import-chapters")]
    public bool AutoImportChapters { get; init; } = false;

    [CommandOption("--import-chapters-file")]
    public string ImportChaptersFile { get; init; } = "";
    
    [CommandOption("--auto-import-cover")]
    public bool AutoImportCover { get; init; } = false;

    [CommandOption("--import-cover-file")]
    public string ImportCoverFile { get; init; } = "";    
    
    
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

    [CommandOption("--meta-encoding-tool")] public string? EncodingTool { get; set; }

    [CommandOption("--meta-genre")] public string? Genre { get; set; }
    [CommandOption("--meta-group")] public string? Group { get; set; }
    
    [CommandOption("--meta-itunes-compilation")]
    public ItunesCompilation? ItunesCompilation { get; set; }

    [CommandOption("--meta-itunes-media-type")]
    public ItunesMediaType? ItunesMediaType { get; set; }

    
    [CommandOption("--meta-itunes-play-gap")]
    public ItunesPlayGap? ItunesPlayGap { get ; set; }


    [CommandOption("--meta-long-description")]
    public string? LongDescription { get; set; }

    [CommandOption("--meta-movement")] public string? Movement { get; set; }
    [CommandOption("--meta-movement-name")] public string? MovementName { get; set; }
    [CommandOption("--meta-narrator")] public string? Narrator { get; set; }
    [CommandOption("--meta-original-album")] public string? OriginalAlbum { get; set; }

    [CommandOption("--meta-original-artist")]
    public string? OriginalArtist { get; set; }

    [CommandOption("--meta-popularity")] public float? Popularity { get; set; }
    [CommandOption("--meta-publisher")] public string? Publisher { get; set; }

    [CommandOption("--meta-publishing-date")]
    public DateTime? PublishingDate { get; set; }

    [CommandOption("--meta-purchase-date")] public DateTime? PurchaseDate { get; set; }

    [CommandOption("--meta-recording-date")] public DateTime? RecordingDate { get; set; }

    [CommandOption("--meta-sort-album")] public string? SortAlbum { get; set; }

    [CommandOption("--meta-sort-album-artist")]
    public string? SortAlbumArtist { get; set; }

    [CommandOption("--meta-sort-artist")] public string? SortArtist { get; set; }
    [CommandOption("--meta-sort-composer")] public string? SortComposer { get; set; }
    [CommandOption("--meta-sort-title")] public string? SortTitle { get; set; }
    [CommandOption("--meta-subtitle")] public string? Subtitle { get; set; }
    [CommandOption("--meta-title")] public string? Title { get; set; }
    [CommandOption("--meta-track-number")] public int? TrackNumber { get; set; }
    [CommandOption("--meta-track-total")] public int? TrackTotal { get; set; }
    
    [CommandOption("--meta-additional-fields")]
    public IDictionary<string, string> AdditionalFields { get; set; } = new Dictionary<string, string>();

    public IDictionary<string, string> MappedAdditionalFields { get; } = new Dictionary<string, string>();

    [CommandOption("--meta-cover")]
    public string[] Covers { get; set; } = Array.Empty<string>();
    
    [CommandOption("--path-pattern|-p")] public string[] PathPattern { get; set; } = Array.Empty<string>();

    [CommandOption("--path-pattern-extension")]
    public string[] PathPatternExtension { get; set; } = Array.Empty<string>();

    
    // fulfil interface contract
    public string? Path => null;
    public TimeSpan TotalDuration => new();
    public IList<ChapterInfo> Chapters { get; } = new List<ChapterInfo>();
    public LyricsInfo? Lyrics { get; set; }
    public IList<PictureInfo> EmbeddedPictures => new List<PictureInfo>();

    
    // todo: https://github.com/spectresystems/spectre.cli/issues/92
    // todo: meta-cover
    /*
    public override ValidationResult Validate()
    {
        return Name.Length < 2
            ? ValidationResult.Error("Names must be at least two characters long")
            : ValidationResult.Success();
    }
    */
}