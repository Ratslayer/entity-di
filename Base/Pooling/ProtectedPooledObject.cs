using System.Collections.Generic;
namespace BB
{
	public abstract class ProtectedPooledObject<TSelf> : IPooledDisposable
		where TSelf : ProtectedPooledObject<TSelf>, new()
	{
		static readonly List<TSelf> _pool = new();
		public ulong Counter { get; private set; }
		protected ProtectedPooledObject() { }
		protected static TSelf GetPooledInternal()
		{

			var result = _pool.Count == 0 ? new TSelf() : _pool.RemoveLast();
			result.Counter = PooledDisposableUtils.GetNextCounter();
			return result;
		}
		public virtual void Dispose()
		{
			if (Counter == 0)
				return;
			Counter = 0;
			_pool.Add((TSelf)this);
		}
		public DisposableToken GetToken() => new(this);
		public DisposableToken<TSelf> GetTypedToken() => new((TSelf)this, Counter);
	}
}