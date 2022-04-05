using System;
using System.Collections.Generic;
using ATL;
using tone.Metadata;

namespace tone.Tests._TestHelpers;


public class MockMetadata: IMetadata
{
    public int? TrackNumber { get; set; }
    public int? TrackTotal { get; set; }
    public ItunesPlayGap? ItunesPlayGap { get; set; }
    public int? DiscNumber { get; set; }
    public int? DiscTotal { get; set; }
    public float? Popularity { get; set; }
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Composer { get; set; }
    public string? Comment { get; set; }
    public string? Genre { get; set; }
    public string? Album { get; set; }
    public string? OriginalAlbum { get; set; }
    public string? OriginalArtist { get; set; }
    public string? Copyright { get; set; }
    public string? Description { get; set; }
    public string? Publisher { get; set; }
    public string? AlbumArtist { get; set; }
    public string? Conductor { get; set; }
    public string? Group { get; set; }
    public string? SortTitle { get; set; }
    public string? SortAlbum { get; set; }
    public string? SortArtist { get; set; }
    public string? SortAlbumArtist { get; set; }
    public string? SortComposer { get; set; }
    public string? LongDescription { get; set; }
    public string? EncodingTool { get; set; }
    public int? Bpm { get; set; }
    public string? EncodedBy { get; set; }
    public string? EncoderSettings { get; set; }
    public string? Subtitle { get; set; }
    public ItunesCompilation? ItunesCompilation { get; set; }
    public ItunesMediaType? ItunesMediaType { get; set; }
    public string? ChaptersTableDescription { get; set; }
    public string? Narrator { get; set; }
    public string? MovementName { get; set; }
    public string? Movement { get; set; }
    public string? Path { get; set; }
    public DateTime? PublishingDate { get; set; }
    public DateTime? RecordingDate { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public LyricsInfo? Lyrics { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public IList<ChapterInfo>? Chapters { get; set; }
    public IList<PictureInfo>? EmbeddedPictures { get; set; }
    public IDictionary<string, string>? AdditionalFields { get; set; }
}