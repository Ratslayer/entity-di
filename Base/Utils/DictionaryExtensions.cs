using System.Collections.Generic;

public static class DictionaryExtensions
{
	public static TValue GetOrCreate<TKey, TValue>(
		this Dictionary<TKey, TValue> dictionary,
		TKey key)
		where TValue : new()
	{
		if (!dictionary.TryGetValue(key, out var result))
		{
			result = new();
			dictionary.Add(key, result);
		}
		return result;
	}
	public static void TryRemoveAndDisposeValue<TKey, TValue>(
		this Dictionary<TKey, TValue> dictionary,
		TKey key)
	{
		dictionary.TryRemoveAndDisposeValue(key);
	}
}
