using System.IO;
using System.Text;

namespace tone.Common.Extensions.String;

public static class StringExtensions
{
    public static System.IO.Stream StringToStream(this string str, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        return new MemoryStream(encoding.GetBytes(str))
        {
            Position = 0
        };
    }
    
    public static string TrimDirectorySeparatorEnd(this string str)
    {
        return str.TrimEnd('/', '\\');
    }
}