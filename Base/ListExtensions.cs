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
	public static bool TryGetNext<T>(this IList<T> list, T element, out T next)
	{
		var found = false;
		foreach (var e in list)
			if (EqualityComparer<T>.Default.Equals(e, element))
				found = true;
			else if (found)
			{
				next = e;
				return true;
			}

		next = default;
		return false;
	}
}
