using System.Collections.Generic;

namespace BB
{
    public static class PooledDisposableExtensions
	{
		public static void RemoveDeadElements<T>(this List<DisposableToken<T>> list)
			where T : IPooledDisposable
		{
			foreach (var i in -list.Count)
				if (!list[i].HasValue(out _))
					list.RemoveAt(i);
		}
		public static void RemoveDeadElements(this List<DisposableToken> list)
		{
			foreach (var i in -list.Count)
				if (!list[i].HasValue(out _))
					list.RemoveAt(i);
		}
	}
}