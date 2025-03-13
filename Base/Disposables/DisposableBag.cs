using System;
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
			foreach (var disposable in _disposables)
				disposable.Dispose();
			_disposables.Clear();
			base.Dispose();
		}
	}
}
