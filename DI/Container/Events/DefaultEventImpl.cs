using System;
using System.Threading;
namespace BB
{
	public sealed class DefaultEventImpl<T> : IEvent<T>, IDisposable
	{
		event Action<T> Events;
		readonly OptimizedCancellationTokenSource _tokenSource;
		public CancellationToken NextEventCancellationToken
			=> _tokenSource.Token;
		public void Dispose()
		{
			Events = null;
			_tokenSource.Dispose();
		}
		public void Publish(T message)
		{
			try
			{
				Events?.Invoke(message);
			}
			catch (Exception e)
			{
				Log.Logger.LogException(e);
			}
			//invoke cancellation token
			_tokenSource.Cancel();
		}
		public void Subscribe(Action<T> action)
		{
			Events += action;
		}

		public void Unsubscribe(Action<T> action)
		{
			Events -= action;
		}
	}
}
