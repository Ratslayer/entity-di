using System;
using System.Collections.Generic;
namespace BB
{
	public sealed class PooledList<T> : List<T>, IDisposableList<T>
	{
		static readonly List<PooledList<T>> _pool = new();
		private PooledList() { }
		public static PooledList<T> GetPooled()
			=> _pool.Count > 0 ? _pool.RemoveLast() : new();
		public void Dispose()
		{
			Clear();
			_pool.Add(this);
		}
	}
	public interface IDisposableList<T> : IList<T>, IDisposable { }
	public static class PooledListExtensions
	{
		public static PooledList<T> ToPooledList<T>(this IEnumerable<T> enumerable)
		{
			var list = PooledList<T>.GetPooled();
			list.AddRange(enumerable);
			return list;
		}
	}
}