using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using tone.Services;
using Xunit;

namespace tone.Tests.Services;
/*
public class GrokPatternServiceTest
{
    [Fact]
    public void TestMultiConcat()
    {
        Assert.Equal("Title", MultiConcat("", " ", "", " - ", "Title"));
        Assert.Equal("1 - Title", MultiConcat("", " ", "1", " - ", "Title"));
        Assert.Equal("Series - Title", MultiConcat("Series", " ", "", " - ", "Title"));
        Assert.Equal("Series 1 - Title", MultiConcat("Series", " ", "1", " - ", "Title"));
    }

    private static string MultiConcat(params string?[] concatStrings)
    {
        var values = concatStrings.Where((_, i) => i%2 ==0).ToImmutableArray();
        var separator = concatStrings.Where((_, i) => i%2!=0).ToImmutableArray();
        var concatValues = new List<string?>();
        for (var i = 0; i < values.Length; i++)
        {
            if (string.IsNullOrEmpty(values[i]))
            {
                continue;
            }
            var separatorIndex = i - 1;
            if (separatorIndex >= 0 && separatorIndex < separator.Length && concatValues.Count > 0)
            {
                concatValues.Add(separator[separatorIndex]);
            }

            concatValues.Add(values[i]);
        }

        return string.Join("", concatValues);
    }



    [Fact]
    public async void TestSuccessfulConditions()
    {
        var subject = new GrokPatternService();

        var definitions = new[]
        {
            "input/%{NOTDIRSEP:genre}/%{NOTDIRSEP:artist}/%{NOTDIRSEP:title}"
        };
        var actual = await subject.BuildAsync(definitions);
        Assert.True(actual);
        Assert.True(actual.Value.Count() == 1);
    }

    [Fact]
    public async void TestFailingConditions()
    {
        var subject = new GrokPatternService();

        var definitions = new[]
        {
            "input/%{NOTDIRSEP:genre}/%{NOTDIRSEP:artist}/%{NOTDIRSEP:title}"
        };

        var customPatterns = new[]
        {
            "NOTDIRSEP [^/\\]*"
        };
        var actual = await subject.BuildAsync(definitions, customPatterns);
        Assert.False(actual);
        Assert.Equal(actual.Error, $"Invalid regex pattern [^/\\]*");
    }
}
*/