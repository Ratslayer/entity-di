namespace BB
{
	public static class IFlushableExtensions
	{
		public static FlushableDisposable FlushOnDispose(this IFlushable flushable)
			=> new(flushable, true);

		public static FlushableDisposable DisableAutoFlush(this IAutoFlushable flushable)
			=> new(flushable, false);

		public static void AutoFlushChangesIfDirty(this IFlushable flushable)
		{
			if (flushable is IAutoFlushable auto && auto.AutoFlushDisabled)
				return;

			if (flushable is not IDirtyFlushable d)
			{
				flushable.ForceFlushChanges();
				return;
			}

			if (!d.IsDirty)
				return;

			d.IsDirty = false;
			d.ForceFlushChanges();
		}
		public static void SetDirtyAndAutoFlushChanges(this IFlushable flushable, bool setDirty = true)
		{
			if (setDirty && flushable is IDirtyFlushable d)
				d.IsDirty = true;
			flushable.AutoFlushChangesIfDirty();
		}
	}
}