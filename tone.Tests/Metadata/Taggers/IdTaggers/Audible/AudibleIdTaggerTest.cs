/*
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Sandreas.AudioMetadata;
using tone.Common.Extensions.Stream;
using tone.Metadata.Formats;
using tone.Metadata.Taggers.IdTaggers.Audible;
using tone.Serializers;
using Xunit;

namespace tone.Tests.Metadata.Taggers.IdTaggers.Audible;

public class AudibleIdTaggerTest
{
    [Fact]
    public async void TestUpdateTag()
    {

        var dataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location + "/../../../../data/");
        var settings = new AudibleIdTaggerSettings()
        {
            MetadataUrlTemplate = new Uri(dataPath + "/Audible/metadata.json").AbsoluteUri,
            ChaptersUrlTemplate =  new Uri(dataPath + "/Audible/subchapters_extra.json").AbsoluteUri
        };
        var http = new HttpClient();
        var tagger = new AudibleIdTagger(settings, http)
        {
            Id = "B0092PNN4O"
        };
        var track = new MetadataTrackHolder();
        var result = await tagger.UpdateAsync(track);
        Assert.Equal("Der Name des Windes", track.Title);
        
        // Assert.Equal(250, track.Chapters.Count);
        
        var x = new ChptFmtNativeMetadataFormat();
        var mem = new MemoryStream();
        await x.WriteAsync(track, mem);
        File.WriteAllText("/home/mediacenter/projects/tone/tone/var/tmp/test.txt", mem.StreamToString());
        Assert.Equal("", mem.StreamToString());
        
    }
}
*/