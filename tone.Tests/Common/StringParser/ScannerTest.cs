using tone.Common.StringParser;
using Xunit;

namespace tone.Tests.Common.StringParser;

public class ScannerTest
{
    const string MultiLineComplex = "abcd\r\nefgh\r\n\r\rijklm\n\n\nabcd";

    [Fact]
    public void TestReadLineMultiLineComplex()
    {
        var scanner = new Scanner(MultiLineComplex);
        Assert.Equal("abcd", scanner.ReadLine());
        Assert.Equal("efgh", scanner.ReadLine());
        Assert.Equal("", scanner.ReadLine());
        Assert.Equal("", scanner.ReadLine());
        Assert.Equal("ijklm", scanner.ReadLine());
        Assert.Equal("", scanner.ReadLine());
        Assert.Equal("", scanner.ReadLine());
        Assert.Equal("abcd", scanner.ReadLine());
    }
}