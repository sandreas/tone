using System.Collections.Generic;

namespace tone.Directives;

public static class LinqExtensions
{
    // https://github.com/Ngineer101/entity-framework-query-filters-linq/blob/master/NWBlog.EntityFrameworkDemo.Api/Extensions/Linq.cs
    // https://github.com/sandreas/tone/compare/main...order-by-experiment
    public static IEnumerable<T> Apply<T>(this IEnumerable<T> src, IDirective<IEnumerable<T>> directive) where T : class
    {
        return directive.Apply(src);
    }
}