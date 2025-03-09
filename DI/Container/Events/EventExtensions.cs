namespace BB
{
	public static class EventExtensions
	{
		public static void Publish<T>(this IEvent<T> e)
			where T : new()
			=> e.Publish(new());
	}
}