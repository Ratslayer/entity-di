using System.Collections.Generic;

public static class DictionaryUtils
{
	public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
		where TValue : new()
	{
		if (!dictionary.TryGetValue(key, out var result))
		{
			result = new();
			dictionary.Add(key, result);
		}
		return result;
	}
}
