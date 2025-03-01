using System;

namespace BB
{
	public readonly struct FlushableDisposable : IDisposable
	{
		readonly IFlushable _flushable;
		readonly bool _autoFlush, _flushOnDispose;
		public FlushableDisposable(IFlushable flushable, bool flushOnDispose)
		{
			_flushable = flushable;
			_flushOnDispose = flushOnDispose;

			if (_flushable is IAutoFlushable auto)
			{
				_autoFlush = auto.AutoFlushDisabled;
				auto.AutoFlushDisabled = true;
			}
			else _autoFlush = default;
		}
		public void Dispose()
		{
			if (_flushable is IAutoFlushable auto)
				auto.AutoFlushDisabled = _autoFlush;

			if (_flushOnDispose)
				_flushable.FlushChangesIfDirty();
		}
	}
}