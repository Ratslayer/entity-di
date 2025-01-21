using System.Collections.Generic;
public static class ListExtensions
{
	public static bool TryRemoveFirst<T>(this IList<T> list, out T element)
	{
		if (list.Count == 0)
		{
			element = default;
			return false;
		}

		element = list[0];
		list.RemoveAt(0);
		return true;
	}
}
