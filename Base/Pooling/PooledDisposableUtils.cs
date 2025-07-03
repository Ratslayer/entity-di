namespace BB
{
	public static class PooledDisposableUtils
	{
		static ulong _counter = 0;
		public static ulong GetNextCounter()
			=> ++_counter;
	}
}