using System;
using System.Collections.Generic;
namespace BB
{
	public sealed class PooledDictionary<TKey,TValue> : Dictionary<TKey, TValue>, IDisposable
	{
		static readonly List<PooledDictionary<TKey,TValue>> _pool = new();
		private PooledDictionary() { }
		public static PooledDictionary<TKey, TValue> GetPooled()
			=> _pool.Count > 0 ? _pool.RemoveLast() : new();
		public void Dispose()
		{
			Clear();
			_pool.Add(this);
		}
	}
}