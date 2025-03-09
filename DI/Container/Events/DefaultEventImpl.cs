using System;
using System.Threading;
namespace BB
{
	public sealed class DefaultEventImpl<T> : IEvent<T>, IDisposable
	{
		event Action<T> Events;
		CancellationTokenSource _tokenSource;
		public CancellationTokenSource CancellationTokenSource
		{
			get
			{
				_tokenSource ??= new CancellationTokenSource();
				return _tokenSource;
			}
		}
		public void Dispose()
		{
			Events = null;
			if (_tokenSource is null)
				return;
			_tokenSource.Dispose();
			_tokenSource = null;
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
			if (_tokenSource is null)
				return;
			_tokenSource.Cancel();
			_tokenSource.Dispose();
			_tokenSource = null;
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
