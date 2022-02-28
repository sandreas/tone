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
}