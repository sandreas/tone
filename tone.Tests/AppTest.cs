using Moq.AutoMock;
using Xunit;

namespace tone.Tests;

public class AppTest
{
    private readonly App _app;

    public AppTest()
    {
        var mocker = new AutoMocker();
        _app = mocker.CreateInstance<App>();
    }
    [Fact]
    public async void TestInvalidCommand()
    {
        // https://stackoverflow.com/questions/48505006/how-to-correctly-write-async-xunit-test
        var actual = await _app.RunAsync(new[] { "tone","invalid-command" });
        Assert.Equal(1, actual);
    }
}