using System;
using System.Collections.Generic;

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

}