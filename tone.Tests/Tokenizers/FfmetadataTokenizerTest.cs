using System.Linq;
using tone.Common.StringParser;
using tone.Tokenizers;
using Xunit;

namespace tone.Tests.Tokenizers;

public class FfmetadataTokenizerTest
{
    private const string SimpleFfmetadata = @";FFMETADATA1
major_brand=isom
minor_version=512
compatible_brands=isomiso2mp41
title=A title
artist=An Artist";
    
    private const string FfmetadataWithChapters = @";FFMETADATA1
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
    
    private readonly FfmetadataTokenizer subject;

    public FfmetadataTokenizerTest()
    {
        
        subject = new FfmetadataTokenizer();
    }
    [Fact]
    public void TestSimpleFfmetadata()
    {
        var tokens = subject.Tokenize(new CharScanner(SimpleFfmetadata));
        Assert.Equal(11, tokens.Count());
    }
    
    [Fact]
    public void TestFfmetadataWithChapters()
    {
        var tokens = subject.Tokenize(new CharScanner(FfmetadataWithChapters));
        Assert.Equal(50, tokens.Count());
    }
}