namespace BB
{
	public static class IFlushableExtensions
	{
		public static FlushableDisposable FlushOnDispose(this IFlushable flushable)
			=> new(flushable);
	}
}