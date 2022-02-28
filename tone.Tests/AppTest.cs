using System.IO;
using Moq;
using Moq.AutoMock;
using tone.Commands;
using tone.Options;
using Xunit;

namespace tone.Tests;
/*
public class AppTest
{
    private readonly AutoMocker _mocker;

    public AppTest()
    {
        _mocker = new AutoMocker();
    }
    [Fact]
    public async void TestInvalidCommand()
    {
        var app = _mocker.CreateInstance<App>();

        // https://stackoverflow.com/questions/48505006/how-to-correctly-write-async-xunit-test
        var actual = await app.RunAsync(new[] { "--invalid-arg" });
        Assert.Equal(1, actual);
    }
    
    [Fact]
    public async void TestMergeError()
    {
        var mockCommand = new Mock<ICommand<MergeOptions>>();
        mockCommand.Setup(x => x.ExecuteAsync(It.IsAny<MergeOptions>())).ReturnsAsync(5);
        var app = new App( mockCommand.Object, new StringWriter());

        var actual = await app.RunAsync(new[] { "mergse" });
        Assert.Equal(5, actual);
    }
}
*/