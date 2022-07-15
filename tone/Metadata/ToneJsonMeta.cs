using System;
using System.Collections.Generic;
using ATL;
using Newtonsoft.Json;
using Sandreas.AudioMetadata;
using tone.Metadata.Converters;

namespace tone.Metadata;

public class ToneJsonMeta: IMetadata
{
    [JsonIgnore]
    public string? Path  => null;
    [JsonIgnore]
    
    public string? BasePath => null;
    [JsonIgnore]

    public TimeSpan TotalDuration => new();
    public string? Album { get; set; }
    public string? AlbumArtist { get; set; }
    public string? Artist { get; set; }
    public int? Bpm { get; set; }
    
    public string? ChaptersTableDescription { get; set; }
    public string? Composer { get; set; }
    public string? Comment { get; set; }
    public string? Conductor { get; set; }
    public string? Copyright { get; set; }
    public string? Description { get; set; }
    public int? DiscNumber { get; set; }
    public int? DiscTotal { get; set; }
    public string? EncodedBy { get; set; }
    public string? EncoderSettings { get; set; }
    public string? EncodingTool { get; set; }
    public string? Genre { get; set; }
    public string? Group { get; set; }
    public ItunesCompilation? ItunesCompilation { get; set; }
    public ItunesMediaType? ItunesMediaType { get; set; }
    public ItunesPlayGap? ItunesPlayGap { get; set; }
    public string? LongDescription { get; set; }
    [JsonConverter(typeof(LyricsConverter))]
    public LyricsInfo? Lyrics { get; set; }
    public string? Part { get; set; }
    public string? Movement { get; set; }
    public string? MovementName { get; set; }
    public string? Narrator { get; set; }
    public string? OriginalAlbum { get; set; }
    public string? OriginalArtist { get; set; }
    public float? Popularity { get; set; }
    public string? Publisher { get; set; }
    public DateTime? PublishingDate { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? RecordingDate { get; set; }
    public string? SortTitle { get; set; }
    public string? SortAlbum { get; set; }
    public string? SortArtist { get; set; }
    public string? SortAlbumArtist { get; set; }
    public string? SortComposer { get; set; }
    public string? Subtitle { get; set; }
    public string? Title { get; set; }
    public int? TrackNumber { get; set; }
    public int? TrackTotal { get; set; }
    [JsonConverter(typeof(ChaptersConverter))]
    public IList<ChapterInfo> Chapters { get; set; } = new List<ChapterInfo>();
    
    [JsonConverter(typeof(PicturesConverter))]
    public IList<PictureInfo> EmbeddedPictures { get; } = new List<PictureInfo>();
    public IDictionary<string, string> AdditionalFields { get; } = new Dictionary<string, string>();
    [JsonIgnore]
    public IDictionary<string, string> MappedAdditionalFields => new Dictionary<string, string>();
}