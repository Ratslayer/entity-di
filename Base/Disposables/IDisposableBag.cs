using System;
using System.Collections;
using System.Collections.Generic;
namespace BB
{
	public sealed class DisposableBag : PooledObject<DisposableBag>
	{
		readonly List<IDisposable> _disposables = new();
		public void AddDisposable(IDisposable disposable)
		{
			if (disposable is not null)
				_disposables.Add(disposable);
		}
		public override void Dispose()
		{
			base.Dispose();
			foreach (var disposable in _disposables)
				disposable.Dispose();
			_disposables.Clear();
		}
	}
	public static class DisposableExtensions
	{
		public static void TryDispose(this object obj)
		{
			if (obj is IDisposable disposable)
				disposable.Dispose();
		}
		public static IDisposable Link(this IDisposable disposable, IDisposable other)
		{
			if (disposable is not DisposableBag bag)
			{
				bag = DisposableBag.GetPooled();
				bag.AddDisposable(disposable);
			}

			bag.AddDisposable(other);
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
