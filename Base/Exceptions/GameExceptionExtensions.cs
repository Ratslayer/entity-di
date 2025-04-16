namespace BB
{
	public static class GameExceptionExtensions
	{
		public static T ThrowIfNull<T>(
			this T obj,
			string message)
			where T : class
		{
			ThrowInternal(obj is null, message);
			return obj;
		}

		private static void ThrowInternal(bool condition, string message, string fallbackMessage = default)
		{
			if (!condition)
				return;

			var msg = string.IsNullOrWhiteSpace(message) ? fallbackMessage : message;
			throw new GameException(msg);
		}
	}
}
