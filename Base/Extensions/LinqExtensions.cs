#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace BB
{
    public static class LinqExtensions
    {
        public static IEnumerable<T2> SelectNotNull<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> converter)
            where T2 : class
            => source
            .Select(converter)
            .Where(t => t is not null);
        public static IEnumerable<T> NotDefault<T>(this IEnumerable<T> source)
            => source.Where(t => !EqualityComparer<T>.Default.Equals(t, default!));
    }
}