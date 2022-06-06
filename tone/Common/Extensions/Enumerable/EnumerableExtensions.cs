using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tone.Common.Extensions.Enumerable;

public static class EnumerableExtensions
{
    public static IEnumerable<T> ForEach<T>(
        this IEnumerable<T> source,
        Action<T> action)
    {
        foreach (T element in source)
        {
            action(element);
            yield return element;
        }
    }
/*
 https://github.com/Ngineer101/entity-framework-query-filters-linq/blob/master/NWBlog.EntityFrameworkDemo.Api/Extensions/Linq.cs
    public static async Task<(int TotalCount, IEnumerable<T> Items)> ToPagedAsync<T>(this IQueryable<T> src, int skip=0, int take=int.MaxValue, string orderBy = "", Func<string, T>? orderByCallback=null) where T : class
    {
        var queryExpression = src.Expression;
        
        if(orderBy != "" && orderByCallback != null){
            var orderByDirectives = orderBy.Split(",").Select(s =>
            {
                var lower = s.Trim().ToLowerInvariant();
                var ascending = !lower.EndsWith(" desc");
                var name = lower.Split(" ").FirstOrDefault()?.Trim() ?? "";
                return (ascending, orderByCallback(name));
            }).Where((t) => t.Item2 != null).ToArray();
        }

        
        queryExpression = queryExpression.OrderBy(orderBy);

        if (queryExpression.CanReduce)
            queryExpression = queryExpression.Reduce();

        src = src.Provider.CreateQuery<T>(queryExpression);

        return (await src.CountAsync(),
            await src.Skip(skip).Take(take).ToListAsync());

    }
*/
}