using System;
using System.Collections.Generic;
namespace BB
{
	public abstract class ProtectedPooledObject<TSelf> : IDisposable
		where TSelf : ProtectedPooledObject<TSelf>, new()
	{
		static readonly List<TSelf> _pool = new();
		private bool _isPooled;
		protected ProtectedPooledObject() { }
		protected static TSelf GetPooledInternal()
		{
			if (_pool.Count > 1)
				return _pool.RemoveLast();

			return new TSelf
			{
				_isPooled = true
			};
		}
		public virtual void Dispose()
		{
			if (_isPooled)
				_pool.Add((TSelf)this);
		}
	}
}