using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using tone.Common.Extensions.String;

namespace tone.Common.Extensions.Enumerable;

public static class EnumerableFileInfoExtensions
{
    
    public static IEnumerable<IFileInfo> OrderBy(
        this IEnumerable<IFileInfo> source,
        string? orderByString)
    {
        if(string.IsNullOrEmpty(orderByString))
        {
            return source;
        }
        
        var orderByDirectives = orderByString.Split(",").Select(s =>
        {
            var lower = s.Trim().ToLowerInvariant();
            var ascending = !lower.EndsWith(" desc");
            var name = lower.TrimSuffix(" asc").TrimSuffix(" desc").Trim();
            return _buildCb(name, ascending);
        }).Where((t) => t.Item1 != null).ToArray();
        
        if(orderByDirectives.Length == 0)
        {
            return source;
        };

        var first = orderByDirectives.First();
        var firstCb = first.Item1!;
        var tmp = first.Item2 ? source.OrderBy(firstCb) : source.OrderByDescending(firstCb);
        var rest = orderByDirectives.Skip(1);
        foreach(var (cb, asc) in orderByDirectives)
        {
            tmp = asc ? tmp.OrderBy(cb!) : tmp.OrderByDescending(cb!);
        };
        return tmp;
    }
    
    
    private static (Func<IFileInfo, object>?, bool) _buildCb(string field, bool asc) =>  field switch 
    {
        "size" => (f => f.Length, asc),
        "filename" => (f => f.Name, asc),
        "extension" => (f => f.Extension, asc),
        "created" => (f => f.CreationTime, asc),
        "modified" => (f => f.LastWriteTime, asc),
        "accessed" => (f => f.LastAccessTime, asc),
        // path or default
        "path" => (f => f.FullName, asc),
        _ => (null, false)
    };

}