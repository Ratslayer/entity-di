using System;
using System.Collections;
using System.Collections.Generic;
namespace BB
{
	public static class DisposableExtensions
	{
		public static void TryDispose(this object obj)
		{
			if (obj is IDisposable disposable)
				disposable.Dispose();
		}
		public static IDisposable LinkDisposable(
			this IDisposable d1,
			IDisposable d2)
		{
			if (d1 is null)
				return d2;
			if (d2 is null)
				return d1;

			if (d1 is DisposableBag b1)
			{
				b1.Add(d2);
				return b1;
			}

			if (d2 is DisposableBag b2)
			{
				b2.Add(d1);
				return b2;
			}

			var bag = DisposableBag.GetPooled();
			bag.Add(d1);
			bag.Add(d2);
			return bag;
		}

		public static void DisposeElementsAndClear(this IList collection)
		{
			foreach (var element in collection)
				if (element is IDisposable disposable)
					disposable.Dispose();
			collection.Clear();
		}
		public static void DisposeAndClear(this List<DisposableToken> collection)
		{
			foreach (var element in collection)
				element.Dispose();
			collection.Clear();
		}
		public static void DisposeAndClear(this IDictionary dictionary)
		{
			foreach (var element in dictionary.Values)
				if (element is IDisposable disposable)
					disposable.Dispose();
			dictionary.Clear();
		}
	}
}
