using BB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
namespace BB
{
	public static class PoolUtils
	{
		static readonly Dictionary<Type, IPool> _pools = new();
		static PoolToken Get<T>(out T obj, Action<object> clear)
			where T : class, new()
		{
			if (!_pools.TryGetValue(typeof(T), out var pool))
			{
				pool = new Pool<T>();
				_pools.Add(typeof(T), pool);
			}
			obj = pool.GetFromPool() as T;
			return PoolToken.Get(obj, pool, clear);
		}
		sealed class Pool<T> : IPool
			where T : class, new()
		{
			readonly List<T> _objects = new();
			public object GetFromPool()
			{
				if (_objects.Count == 0)
					return new T();
				var result = _objects[^1];
				_objects.RemoveAt(_objects.Count - 1);
				return result;
			}

			public void ReturnToPool(object obj)
			{
				if (obj is not T t)
					return;
				_objects.Add(t);
			}
		}
		static PoolToken Link(this PoolToken token, PoolToken other)
		{
			token.Link(other);
			return token;
		}

		static readonly Action<object> _clear = obj =>
		{
			switch (obj)
			{
				case IList list:
					list.Clear();
					break;
				case IDictionary dictionary:
					dictionary.Clear();
					break;
				case StringBuilder sb:
					sb.Clear();
					break;
				case IDisposable disposable:
					disposable.Dispose();
					break;
				case IClearable clearable:
					clearable.Clear();
					break;
			}
		};

		public static PoolToken GetDisposable<T>(out T disposable)
			where T : class, IDisposable, new()
			=> Get(out disposable, _clear);
		public static PoolToken GetDisposable<T>(this PoolToken token, out T disposable)
			where T : class, IDisposable, new()
			=> Link(token, GetDisposable(out disposable));

		public static PoolToken GetClearable<T>(out T clearable)
			where T : class, IClearable, new()
			=> Get(out clearable, _clear);
		public static PoolToken GetClearable<T>(this PoolToken token, out T clearable)
			where T : class, IClearable, new()
			=> Link(token, GetClearable(out clearable));
	}
}