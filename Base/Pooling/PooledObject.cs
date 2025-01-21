using System;
using System.Collections.Generic;

public abstract class PooledObject<TSelf> : IDisposable
	where TSelf : PooledObject<TSelf>, new()
{
	static readonly List<TSelf> _pool = new();
	private bool _isPooled;
	protected PooledObject() { }
	public static TSelf GetPooled()
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
