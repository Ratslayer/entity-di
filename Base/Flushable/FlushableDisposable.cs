using System;

namespace BB
{
	public readonly struct FlushableDisposable : IDisposable
	{
		readonly IFlushable _flushable;
		readonly bool _autoFlush;
		public FlushableDisposable(IFlushable flushable)
		{
			_flushable = flushable;
			_autoFlush = default;

			if (_flushable is IAutoFlushable auto)
			{
				_autoFlush = auto.AutoFlushDisabled;
				auto.AutoFlushDisabled = true;
			}
		}
		public void Dispose()
		{
			_flushable.FlushChanges();
			if (_flushable is IAutoFlushable auto)
				auto.AutoFlushDisabled = _autoFlush;
		}
	}
}