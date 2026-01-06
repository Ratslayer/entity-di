#nullable enable
using System;
using System.Collections;
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
    }
    public readonly struct EnumerableAdapter<T> : IEnumerable<T>
    {
        readonly T? _value;
        readonly IEnumerable<T> _values;
        private EnumerableAdapter(T? value, IEnumerable<T> values)
        {
            _value = value;
            _values = values;
        }
        public IEnumerator<T> GetEnumerator()
        {
            if (_value is not null)
                yield return _value;
            if (_values is not null)
                foreach (var value in _values)
                    yield return value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}