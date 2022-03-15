
using tone.Common.Extensions.String;
using tone.Metadata.Formats;
using Xunit;

namespace tone.Tests.Metadata.Formats;

public class FfmetadataFormatTest
{
    const string SimpleFfmetadata = @";FFMETADATA1
major_brand=isom
minor_version=512
compatible_brands=isomiso2mp41
title=A title
artist=An Artist
composer=A composer
album=An Album
date=2011
description=A description
comment=A comment
encoder=Lavf56.40.101
[CHAPTER]
TIMEBASE=1/1000
START=0
END=264034
title=001
[CHAPTER]
TIMEBASE=1/1000
START=264034
END=568958
title=002
[CHAPTER]
TIMEBASE=1/1000
START=568958
END=879455
title=003";
    
    private readonly FfmetadataFormat _subject;
 
    
    public FfmetadataFormatTest()
    {
        _subject = new FfmetadataFormat();
    }

    [Fact]
    public async void TestReadSimple()
    {
        await using var stream = SimpleFfmetadata.StringToStream();
        var actual = await _subject.ReadAsync(stream);
        Assert.True(actual);
    }


}