using System;
using System.Collections;
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

			if(d1 is DisposableBag b1)
			{
				b1.AddDisposable(d2);
				return b1;
			}

			if(d2 is DisposableBag b2)
			{
				b2.AddDisposable(d1);
				return b2;
			}

			var bag = DisposableBag.GetPooled();
			bag.AddDisposable(d1);
			bag.AddDisposable(d2);
			return bag;
		}

		public static void DisposeAndClear(this IList collection)
		{
			foreach (var element in collection)
				if (element is IDisposable disposable)
					disposable.Dispose();
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
