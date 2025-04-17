using System;

namespace BB
{
	public sealed class GameException : Exception
	{
		public GameException(
			string message,
			Exception innerException = null)
			: base(message, innerException)
		{
		}
	}
}