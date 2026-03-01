using System;

namespace BB
{
    public readonly struct DisposableToken<T> : IDisposable
		where T : IPooledDisposable
	{
		readonly T _disposable;
		readonly ulong _counter;
		public bool HasValue(out T value)
		{
			var result = (bool)this;
			value = result ? _disposable : default;
			return result;
		}
		public T Value
		{
			get
			{
				HasValue(out var result);
				return result;
			}
		}
		public DisposableToken(T disposable, ulong counter)
		{
			_disposable = disposable;
			_counter = counter;
		}
		public void Dispose()
		{
			if (this)
				_disposable.Dispose();
		}
		public static implicit operator bool(DisposableToken<T> d)
			=> d._disposable is not null
			&& d._counter > 0
			&& d._counter == d._disposable.Counter;
		public static implicit operator DisposableToken(DisposableToken<T> d)
			=> new(d._disposable, d._counter);
	}
}