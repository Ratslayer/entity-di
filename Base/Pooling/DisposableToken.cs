using System;

namespace BB
{
    public static class DisposableTokenExtensions
    {
        public static DisposableTokens Add(this DisposableToken token1, DisposableToken token2)
        {
            var list = PooledList<DisposableToken>.GetPooled();
            list.Add(token1);
            list.Add(token2);
            return new DisposableTokens(list);
        }
        public static DisposableTokens Add(this DisposableToken token, IPooledDisposable disposable)
           => token.Add(new DisposableToken(disposable));
        public static DisposableTokens Add(this DisposableTokens tokens, DisposableToken token)
        {
            tokens.Add(token);
            return tokens;
        }
        public static DisposableTokens Add(this DisposableTokens tokens, IPooledDisposable disposable)
           => tokens.Add(new DisposableToken(disposable));
    }
    public readonly struct DisposableTokens : IDisposable
    {
        readonly PooledList<DisposableToken> _tokens;
        readonly DisposableToken _listToken;
        public DisposableTokens(PooledList<DisposableToken> tokens)
        {
            _tokens = tokens;
            _listToken = new(_tokens);
        }
        public void Dispose()
        {
            if (_tokens is null)
                return;
            if (!_listToken.CanDispose)
                return;
            foreach (var token in _tokens)
                token.Dispose();
            _tokens.Dispose();
        }
        public void Append(DisposableToken token)
            => _tokens.Add(token);
        public static implicit operator DisposableTokens(DisposableToken token)
        {
            var list = PooledList<DisposableToken>.GetPooled();
            list.Add(token);
            return new(list);
        }

    }
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
            IDisposable => true,
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