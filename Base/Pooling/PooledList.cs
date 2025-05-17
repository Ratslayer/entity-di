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
}