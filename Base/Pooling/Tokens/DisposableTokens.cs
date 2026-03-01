using System;

namespace BB
{
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
}