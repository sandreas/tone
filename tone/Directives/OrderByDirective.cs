using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace tone.Directives;

public class OrderByDirective: IDirective<IEnumerable<IFileInfo>>
{
    private readonly (Func<IFileInfo,IComparable> comparator, bool ascending)[] _orderBy;

    public OrderByDirective(string orderBy):this(orderBy, _buildCb)
    {
        
    }

    public OrderByDirective(string orderBy, Func<string, Func<IFileInfo, IComparable>> keySelector)
    {
        _orderBy = orderBy.Split(",").Distinct().Select(s =>
        {
            var lower = s.Trim().ToLowerInvariant();
            var ascending = !lower.StartsWith("-");
            var name = lower.TrimStart('-', '+').Trim();
            return (keySelector(name), ascending);
        }).ToArray();
    }
    
    public IEnumerable<IFileInfo> Apply(IEnumerable<IFileInfo> subject)
    {
        if (_orderBy.Length == 0)
        {
            return subject;
        }
        var first = _orderBy.First();
        
        var tmp = first.ascending ? subject.OrderBy(first.comparator) : subject.OrderByDescending(first.comparator);
        var rest = _orderBy.Skip(1);
        // var x = DateTime.Now.CompareTo(DateTime.Now);
        foreach(var (cb, asc) in rest)
        {
            tmp = asc ? tmp.OrderBy(cb) : tmp.OrderByDescending(cb);
        }
        return tmp;
    }
    
    private static Func<IFileInfo, IComparable> _buildCb(string field) =>  field switch 
    {
        "size" => f => f.Length,
        "filename" => f => f.Name,
        "extension" => f => f.Extension,
        "created" => f => f.CreationTime,
        "modified" => f => f.LastWriteTime,
        "accessed" => f => f.LastAccessTime,
        // path or default
        _ => f => f.FullName
    };
    
}
