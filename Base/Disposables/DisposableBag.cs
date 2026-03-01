using System;
using System.Collections.Generic;
namespace BB
{
    public sealed class DisposableBag : PooledObject<DisposableBag>
    {
        readonly List<IDisposable> _disposables = new();
        public DisposableBag Add(IDisposable disposable)
        {
            if (disposable is not null)
                _disposables.Add(disposable);
            return this;
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
