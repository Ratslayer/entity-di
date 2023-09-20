using System;

namespace EntityDi;
public interface ISubscriber<T>
{
	void Subscribe(Action<T> action);
	void Unsubscribe(Action<T> action);
}
public interface IPublisher<T>
{
	void Publish(T message);
}
public sealed class Event<T> : ISubscriber<T>, IPublisher<T>
{
	event Action<T> _onPublish;
	public void Publish(T message)
	{
		_onPublish?.Invoke(message);
	}

	public void Subscribe(Action<T> action)
	{
		_onPublish += action;
	}

	public void Unsubscribe(Action<T> action)
	{
		_onPublish -= action;
	}
}
public static class EventExtensions
{
	public static void Publish<T>(this IPublisher<T> e)
		where T : new()
	{
		e.Publish(new());
	}
	public static void Publish<T>(this IEntity entity, T msg)
	{
		if (entity.TryResolve(out IPublisher<T> e))
			e.Publish(msg);

	}
}