using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATL;
using tone.Common.Extensions.Stream;
using tone.Common.Extensions.String;
using tone.Metadata;
using tone.Metadata.Formats;
using Xunit;

namespace tone.Tests.Metadata.Formats;

public class ChptFmtNativeMetadataFormatTest
{
    private readonly ChptFmtNativeMetadataFormat _subject;

    const string SimpleChapters = @"00:00:00.000 chapter 1
00:00:00.500 chapter 2
00:00:01.000 chapter 3";
    
    const string SimpleChaptersWithTotalLength = @"# total-length 00:00:01.500
00:00:00.000 chapter 1
00:00:00.500 chapter 2
00:00:01.000 chapter 3";
    
    const string SimpleChaptersWithTotalDuration = @"## total-duration: 00:00:01.500
00:00:00.000 chapter 1
00:00:00.500 chapter 2
00:00:01.000 chapter 3";

    const string SimpleChaptersOutput = @"## total-duration: 00:00:20.000
00:00:00.000 chapter 1
00:00:05.000 chapter 2
00:00:15.000 chapter 3";    
    
    public ChptFmtNativeMetadataFormatTest()
    {
        _subject = new ChptFmtNativeMetadataFormat();
    }
    
    [Fact]
    public async void TestReadAsyncSimpleChapters()
    {
        await using var stream = SimpleChapters.StringToStream();
        var actual = await _subject.ReadAsync(stream);
        Assert.True(actual);
        var chapters = actual.Value.Chapters ?? new List<ChapterInfo>();
        Assert.Equal(3, chapters.Count);
        
        Assert.Equal(0u, chapters[0].StartTime);
        Assert.Equal(500u, chapters[0].EndTime);
        Assert.Equal("chapter 1", chapters[0].Title);

        Assert.Equal(500u, chapters[1].StartTime);
        Assert.Equal(1000u, chapters[1].EndTime);
        Assert.Equal("chapter 2", chapters[1].Title);      
        
        Assert.Equal(1000u, chapters[2].StartTime);
        Assert.Equal(0u, chapters[2].EndTime);
        Assert.Equal("chapter 3", chapters[2].Title);
    }
    
    [Fact]
    public async void TestReadAsyncSimpleChaptersWithTotalLength()
    {
        await using var stream = SimpleChaptersWithTotalLength.StringToStream();
        var actual = await _subject.ReadAsync(stream);
        Assert.True(actual);

        var chapters = actual.Value.Chapters ?? new List<ChapterInfo>();
        Assert.Equal(3, chapters.Count);
        
        Assert.Equal(0u, chapters[0].StartTime);
        Assert.Equal(500u, chapters[0].EndTime);
        Assert.Equal("chapter 1", chapters[0].Title);

        Assert.Equal(500u, chapters[1].StartTime);
        Assert.Equal(1000u, chapters[1].EndTime);
        Assert.Equal("chapter 2", chapters[1].Title);      
        
        Assert.Equal(1000u, chapters[2].StartTime);
        Assert.Equal(1500u, chapters[2].EndTime);
        Assert.Equal("chapter 3", chapters[2].Title);
    }
    
    
    [Fact]
    public async void TestReadAsyncSimpleChaptersWithTotalDuration()
    {
        await using var stream = SimpleChaptersWithTotalDuration.StringToStream();
        var actual = await _subject.ReadAsync(stream);
        Assert.True(actual);
        var chapters = actual.Value.Chapters ?? new List<ChapterInfo>();
        Assert.Equal(3, chapters.Count);
        
        Assert.Equal(0u, chapters[0].StartTime);
        Assert.Equal(500u, chapters[0].EndTime);
        Assert.Equal("chapter 1", chapters[0].Title);

        Assert.Equal(500u, chapters[1].StartTime);
        Assert.Equal(1000u, chapters[1].EndTime);
        Assert.Equal("chapter 2", chapters[1].Title);      
        
        Assert.Equal(1000u, chapters[2].StartTime);
        Assert.Equal(1500u, chapters[2].EndTime);
        Assert.Equal("chapter 3", chapters[2].Title);
    }
 
    [Fact]
    public async void TestWriteAsync()
    {
        var metadata = new MetadataTrack
        {
            Chapters = new List<ChapterInfo>()
        };
        metadata.Chapters.Add(new ChapterInfo(0, "chapter 1"));
        metadata.Chapters.Add(new ChapterInfo(5000, "chapter 2"));
        metadata.Chapters.Add(new ChapterInfo(15000, "chapter 3"));
        metadata.Chapters.Last().EndTime = 20000;
        var outputStream = new MemoryStream();

        Assert.True(await _subject.WriteAsync(metadata, outputStream));
        Assert.Equal(SimpleChaptersOutput, outputStream.StreamToString());
    }
    
}