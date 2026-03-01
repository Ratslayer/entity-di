using System.Collections.Generic;
namespace BB
{
    public abstract class ProtectedPooledObject<TSelf> : BasePooledObject
        where TSelf : ProtectedPooledObject<TSelf>, new()
    {
        static readonly List<TSelf> _pool = new();
        ulong _counter;
        public override ulong Counter => _counter;
        protected ProtectedPooledObject() { }
        protected static TSelf GetPooledInternal()
        {

            var result = _pool.Count == 0 ? new TSelf() : _pool.RemoveLast();
            result._counter = PooledDisposableUtils.GetNextCounter();
            return result;
        }
        public override void Dispose()
        {
            if (Counter == 0)
                return;
            _counter = 0;
            _pool.Add((TSelf)this);
        }
        public DisposableToken GetToken() => new(this);
        public DisposableToken<TSelf> GetTypedToken() => new((TSelf)this, Counter);
    }
}