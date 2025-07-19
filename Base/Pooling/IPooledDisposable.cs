using System;
using System.Collections.Generic;

namespace BB
{
	public interface IPooledDisposable : IDisposable
	{
		ulong Counter { get; }
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
			if (_disposable is null)
				return;
			if (_disposable is IPooledDisposable pd
				&& (_counter == 0 || pd.Counter != _counter))
				return;

			_disposable.Dispose();
		}
		public bool HasValue(out IDisposable disposable)
		{
			disposable = _disposable;
			return _disposable is IPooledDisposable pd
			? _counter != 0 && pd.Counter == _counter
			: _disposable is not null;
		}
	}
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
	public static class PooledDisposableExtensions
	{
		public static void RemoveDeadElements<T>(this List<DisposableToken<T>> list)
			where T : IPooledDisposable
		{
			foreach (var i in -list.Count)
				if (!list[i].HasValue(out _))
					list.RemoveAt(i);
		}
		public static void RemoveDeadElements(this List<DisposableToken> list)
		{
			foreach (var i in -list.Count)
				if (!list[i].HasValue(out _))
					list.RemoveAt(i);
		}
	}
}