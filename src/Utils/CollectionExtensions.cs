namespace EntityDi;

public static class CollectionExtensions
{
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
}
