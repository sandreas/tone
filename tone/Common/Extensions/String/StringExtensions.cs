using System.IO;
using System.Text;

namespace tone.Common.Extensions.String;

public static class StringExtensions
{
    public static string TrimPrefix(this string s, string prefix)
    {
        return s.StartsWith(prefix) ? s[prefix.Length..] : s;
    }
    
    public static string TrimSuffix(this string s, string suffix)
    {
        return s.EndsWith(suffix) ? s[..^suffix.Length] : s;
    }
    
    public static System.IO.Stream StringToStream(this string str, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        return new MemoryStream(encoding.GetBytes(str));
    }
}