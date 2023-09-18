using System;
using System.Collections.Generic;

namespace EntityDi;
public static class CollectionExtensions
{
	public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> creator)
	{
		if (!dictionary.TryGetValue(key, out TValue value))
		{
			value = creator();
			dictionary.Add(key, value);
		}
		return value;
	}
	public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
		where TValue : new()
	{
		if(!dictionary.TryGetValue(key, out TValue value))
		{
			value = new();
			dictionary.Add(key, value);
		}
		return value;
	}
	public static bool TryGet<T>(this IEnumerable<T> col, out T element, Predicate<T> filter)
	{
		foreach (var item in col)
			if (filter(item))
			{
				element = item;
				return true;
			}
		element = default;
		return false;
	}
	public static bool Contains<T>(this IEnumerable<T> col, Predicate<T> predicate)
	{
		foreach (var item in col)
			if (predicate(item))
				return true;
		return false;
	}
}
