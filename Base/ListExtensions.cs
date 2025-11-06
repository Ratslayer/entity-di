using System;
using System.Collections.Generic;
using System.Linq;
public static class LinqExtensions
{
    public static IEnumerable<T2> SelectNotNull<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> converter)
        where T2 : class
        => source
        .Select(converter)
        .Where(t => t is not null);
}
public static class ListExtensions
{
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> collection)
        => collection ?? Enumerable.Empty<T>();
    public static bool Pop<T>(this IList<T> list, T item)
    {
        for (var i = list.Count - 1; i >= 0; i--)
            if (EqualityComparer<T>.Default.Equals(list[i], item))
            {
                list.RemoveAt(i);
                return true;
            }
        return false;
    }
    public static bool TryRemoveFirst<T>(this IList<T> list, out T element)
    {
        if (list.Count == 0)
        {
            element = default;
            return false;
        }

        element = list[0];
        list.RemoveAt(0);
        return true;
    }
    public static T RemoveFirstOrDefault<T>(this IList<T> list, Func<T, bool> predicate)
    {
        foreach (var i in list.Count)
        {
            var element = list[i];
            if (!predicate(element))
                continue;
            list.RemoveAt(i);
            return element;
        }
        return default;
    }
    public static bool TryGetNext<T>(this IList<T> list, T element, out T next)
    {
        var found = false;
        foreach (var e in list)
            if (EqualityComparer<T>.Default.Equals(e, element))
                found = true;
            else if (found)
            {
                next = e;
                return true;
            }

        next = default;
        return false;
    }
    public static bool TryRemoveLast<T>(this IList<T> list, out T element)
    {
        if (list.Count == 0)
        {
            element = default;
            return false;
        }

        element = list[^1];
        list.RemoveAt(list.Count - 1);
        return true;
    }
}
