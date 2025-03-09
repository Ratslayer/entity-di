using System;
using System.Threading;
namespace BB
{
	public interface IEvent<T>
	{
		CancellationTokenSource CancellationTokenSource { get; }
		void Publish(T message);
		void Subscribe(Action<T> action);
		void Unsubscribe(Action<T> action);
	}
}
