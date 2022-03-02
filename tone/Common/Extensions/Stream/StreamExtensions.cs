using System.IO;
using System.Text;

namespace tone.Common.Extensions.Stream;

public static class StreamExtensions
{
    public static string StreamToString(this System.IO.Stream stream, Encoding? encoding = null)
    {
        stream.Position = 0;
        return new StreamReader(stream).ReadToEnd();
    }
}