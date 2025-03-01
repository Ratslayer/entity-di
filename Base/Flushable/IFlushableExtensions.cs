using Unity.VisualScripting;

namespace BB
{
	public static class IFlushableExtensions
	{
		public static FlushableDisposable FlushOnDispose(this IFlushable flushable)
			=> new(flushable, true);

		public static FlushableDisposable DisableAutoFlush(this IAutoFlushable flushable)
			=> new(flushable, false);

		public static void FlushChangesIfDirty(this IFlushable flushable)
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
		public static void SetDirtyAndFlushChanges(this IFlushable flushable, bool setDirty = true)
		{
			if (setDirty && flushable is IDirtyFlushable d)
				d.IsDirty = true;
			flushable.FlushChangesIfDirty();
		}
		//public static void AutoFlushChanges(this IAutoFlushable flushable)
		//{
		//	if (!flushable.AutoFlushDisabled)
		//		flushable.FlushChangesIfDirty();
		//}
	}
}