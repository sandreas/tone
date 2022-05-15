using System.Collections.Generic;
using System.IO;
using System.Text;
using GrokNet;
using tone.Metadata.Taggers;
using tone.Tests._TestHelpers;
using Xunit;

namespace tone.Tests.Metadata.Taggers;

public class PathPatternTaggerTest
{
    [Fact]
    public async void TestSimpleConditions()
    {
        var metadata = new MockMetadata
        {
            Path = "input/Fantasy/J.K. Rowling/Quidditch Through the Ages/"
        };
        var customPatternStream = new MemoryStream(Encoding.Default.GetBytes("NOTDIRSEP [^/\\\\]*"));
        var grokDefinitions = new List<Grok>()
        {
            new("input/%{NOTDIRSEP:genre}/%{NOTDIRSEP:artist}/%{NOTDIRSEP:title}", customPatternStream),
        };
        /*
        var subject = new PathPatternTagger(grokDefinitions);
        var actual = await subject.UpdateAsync(metadata);
        
        Assert.True(actual);
        Assert.Equal("Fantasy", metadata.Genre);
        Assert.Equal("J.K. Rowling", metadata.Artist);
        Assert.Equal("Quidditch Through the Ages", metadata.Title);
        */
    }
}