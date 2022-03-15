using System.Linq;
using tone.Services;
using Xunit;

namespace tone.Tests.Services;

public class GrokPatternServiceTest
{
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