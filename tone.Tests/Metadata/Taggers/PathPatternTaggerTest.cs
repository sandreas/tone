using System;
using System.Collections.Generic;
using ATL;
using Moq;
using tone.Metadata;
using tone.Metadata.Taggers;
using Xunit;

namespace tone.Tests.Metadata.Taggers;

class MockMeta: IMetadata
{
    public int? TrackNumber { get; set; }
    public int? TrackTotal { get; set; }
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
    public string? LongDescription { get; set; }
    public string? EncodingTool { get; set; }
    public string? MediaType { get; set; }
    public string? ChaptersTableDescription { get; set; }
    public string? Narrator { get; set; }
    public string? SeriesTitle { get; set; }
    public string? SeriesPart { get; set; }
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
public class PathPatternTaggerTest
{
    [Fact]
    public async void TestSimplePatternReplacements()
    {
        var metadata = new MockMeta()
        {
            Path = "input/Fantasy/J.K. Rowling/Quidditch Through the Ages/"
        };
        var grokDefinitions = new[]
        {
            "input/%g/%a/%n"
        };
        var defaultCustomPatterns = new[]
        {
            "NOTDIRSEP [^/\\\\]*",
        };
        var subject = new PathPatternTagger(grokDefinitions, defaultCustomPatterns);
        var actual = await subject.Update(metadata);
        
        Assert.True(actual);
        Assert.Equal("Fantasy", metadata.Genre);
        Assert.Equal("J.K. Rowling", metadata.Artist);
        Assert.Equal("Quidditch Through the Ages", metadata.Title);
    }

    [Fact]
    public async void TestSimplePattern()
    {
        var metadata = new MockMeta()
        {
            Path = "input/Fantasy/J.K. Rowling/Quidditch Through the Ages/"
        };
        var grokDefinitions = new string[]
        {
            // "input/%{NONSLASH:genre}/%{NONSLASH:author}/%{NONSLASH:series}/%{WORD:part} - %{NONSLASH:title}"
            "input/%{NOTDIRSEP:genre}/%{NOTDIRSEP:artist}/%{NOTDIRSEP:title}"
        };
        var defaultCustomPatterns = new[]
        {
            "NOTDIRSEP [^/\\\\]*",
        };
        var subject = new PathPatternTagger(grokDefinitions, defaultCustomPatterns);
        var actual = await subject.Update(metadata);
        
        Assert.True(actual);
        Assert.Equal("Fantasy", metadata.Genre);
        Assert.Equal("J.K. Rowling", metadata.Artist);
        Assert.Equal("Quidditch Through the Ages", metadata.Title);
    }
}