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
}