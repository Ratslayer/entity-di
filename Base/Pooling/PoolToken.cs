using System;

public sealed class PoolToken : PooledObject<PoolToken>
{
	object _object;
	IPool _pool;
	Action<object> _dispose;
	public static PoolToken Get(object obj, IPool pool, Action<object> dispose)
	{

		var result = GetPooled();

		result._object = obj;
		result._pool = pool;
		result._dispose = dispose;

		return result;
	}
	public override void Dispose()
	{
		_dispose?.Invoke(_object);
		_pool.ReturnToPool(_object);

		base.Dispose();
	}
}
