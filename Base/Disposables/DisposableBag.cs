using System;
using System.Collections.Generic;
namespace BB
{
    public sealed class DisposableAction : ProtectedPooledObject<DisposableAction>
    {
        Action _action;
        public static DisposableAction GetPooled(Action action)
        {
            var result = GetPooledInternal();
            result._action = action;
            return result;
        }
        public override void Dispose()
        {
            _action?.Invoke();
            base.Dispose();
        }
    }
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
