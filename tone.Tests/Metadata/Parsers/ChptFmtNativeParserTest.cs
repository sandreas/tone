using System.Collections.Generic;
using ATL;
using tone.Metadata.Parsers;
using Xunit;

namespace tone.Tests.Metadata.Parsers;

public class ChptFmtNativeParserTest
{
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
    private readonly ChptFmtNativeParser _subject;

    public ChptFmtNativeParserTest()
    {
        _subject = new ChptFmtNativeParser();
    }
    
    [Fact]
    public async void TestParseSimpleChapters()
    {
        await using var stream = TestUtil.GenerateStreamFromString(SimpleChapters);
        var actual = _subject.Parse(stream);
        var chapters = actual.Chapters ?? new List<ChapterInfo>();
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
    public async void TestParseSimpleChaptersWithTotalLength()
    {
        await using var stream = TestUtil.GenerateStreamFromString(SimpleChaptersWithTotalLength);
        var actual = _subject.Parse(stream);
        var chapters = actual.Chapters ?? new List<ChapterInfo>();
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
    public async void TestParseSimpleChaptersWithTotalDuration()
    {
        await using var stream = TestUtil.GenerateStreamFromString(SimpleChaptersWithTotalDuration);
        var actual = _subject.Parse(stream);
        var chapters = actual.Chapters ?? new List<ChapterInfo>();
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
    
}