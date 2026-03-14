using System;

namespace BB
{
    public readonly struct DisposableToken : IDisposable
    {
        readonly IDisposable _disposable;
        readonly ulong _counter;
        public DisposableToken(IDisposable disposable)
        {
            _disposable = disposable;
            _counter = disposable is IPooledDisposable pd
                ? pd.Counter : 0;
        }
        public DisposableToken(IDisposable disposable, ulong counter)
        {
            _disposable = disposable;
            _counter = counter;
        }
        public void Dispose()
        {
            if (CanDispose)
                _disposable.Dispose();
        }
        public bool CanDispose => _disposable switch
        {
            IPooledDisposable pd => _counter != 1 && pd.Counter == _counter,
            not null => true,
            _ => false
        };
        public bool HasValue(out IDisposable disposable)
        {
            disposable = _disposable;
            return _disposable is IPooledDisposable pd
            ? _counter != 0 && pd.Counter == _counter
            : _disposable is not null;
        }
    }
}