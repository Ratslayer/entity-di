using BB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
//public static class DictionaryPool
//{
//	public static PoolToken<Dictionary<TKey, TValue>> Get<TKey, TValue>(out Dictionary<TKey, TValue> dictionary)
//		=> PoolUtils.Get(out dictionary, d => d.Clear());
//}
//public static class ListPool
//{
//	public static PoolToken<List<T>> Get<T>(out List<T> list)
//		=> PoolUtils.Get(out list, l => l.Clear());
//}
//public static class StringBuilderPool
//{
//	public static PoolToken<StringBuilder> Get(out StringBuilder builder)
//		=> PoolUtils.Get(out builder, b => b.Clear());
//}
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

	//public static PoolToken Get<T>(out List<T> list)
	//	=> Get(out list, _clear);
	//public static PoolToken Get<T>(this PoolToken token, out List<T> list)
	//	=> Link(token, Get(out list));

	//public static PoolToken Get<TKey, TValue>(out Dictionary<TKey, TValue> dictionary)
	//	=> Get(out dictionary, _clear);
	//public static PoolToken Get<TKey, TValue>(this PoolToken token, out Dictionary<TKey, TValue> dictionary)
	//	=> Link(token, Get(out dictionary));

	//public static PoolToken GetStringBuilder(out StringBuilder builder)
	//	=> Get(out builder, _clear);
	//public static PoolToken GetStringBuilder(this PoolToken token, out StringBuilder builder)
	//	=> Link(token, GetStringBuilder(out builder));

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
public interface IPool
{
	public object GetFromPool();
	public void ReturnToPool(object obj);
}
