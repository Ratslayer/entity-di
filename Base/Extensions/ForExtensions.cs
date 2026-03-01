using System;
using System.Collections.Generic;
public static class ForExtensions
{
    public static CustomIListEnumerator<T> GetEnumerator<T>(
        this (int start, IList<T> list) p)
        => new(p.list, p.start, p.list.Count - 1);
    public static CustomIntEnumerator GetEnumerator(this Range range)
        => new(range.Start.Value, range.End.Value);
    public static CustomIntEnumerator GetEnumerator(this int? n)
    {
        if (n is null)
            return new(false);
        return GetEnumerator(n ?? 0);
    }
    public static CustomIntEnumerator GetEnumerator(this int n)
        => n > 0
        ? new(0, n - 1)
        : n < 0
        ? new(-n - 1, 0)
        : new(false);
    public static int Indices<T>(this IReadOnlyList<T> list)
        => list.Count;
    public static int IndicesReversed<T>(this IReadOnlyList<T> list)
        => -list.Count;

    public static IEnumerable<int> IndicesWithOffset<T>(this IReadOnlyList<T> list, int offset)
    {
        if (list?.Count is null or 0)
            yield break;
        for (var i = 0; i < list.Count; i++)
            yield return (i + offset) % list.Count;
    }
    public static IEnumerable<int> IndicesWithRandomOffset<T>(this IReadOnlyList<T> list)
        => list.IndicesWithOffset(RandomUtils.Range(0, list.Count));
    public static IEnumerable<T> WithOffset<T>(this IReadOnlyList<T> list, int offset)
    {
        if (list?.Count is null or 0)
            yield break;
        for (var i = 0; i < list.Count; i++)
            yield return list[(i + offset) % list.Count];
    }
    public static IEnumerable<T> WithRandomOffset<T>(this IReadOnlyList<T> list)
        => list.WithOffset(RandomUtils.Range(0, list.Count));
}