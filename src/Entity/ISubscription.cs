using EntityDi.Container;
using System;

namespace EntityDi;

public interface ISubscription
{
	void Init(IEntity entity);
	void Subscribe();
	void Unsubscribe();
}
public interface IAttachedSubscription
{
	void Subscribe(IEntity entity);
	void Unsubscribe();
}
public sealed record AttachedEventSubscription<T>(Action<T> Action) : IAttachedSubscription
{
	ISubscriber<T> _subscriber;
	public void Subscribe(IEntity entity)
	{
		if (entity.TryResolve(out IPublisher<T> publisher))
		{
			_subscriber = (ISubscriber<T>)publisher;
			_subscriber.Subscribe(Action);
		}
		else _subscriber = null;
	}

	public void Unsubscribe()
	{
		_subscriber?.Unsubscribe(Action);
	}
}
public sealed record EventSubscription<T>(Action<T> Action) : ISubscription
{
	IPublisher<T> _subscriber;
	public void Init(IEntity entity) => _subscriber = entity.Resolve<IPublisher<T>>();

	public void Unsubscribe()
	{
		((ISubscriber<T>)_subscriber).Unsubscribe(Action);
	}
	public void Subscribe()
	{
		((ISubscriber<T>)_subscriber).Subscribe(Action);
	}
}
[AttributeUsage(AttributeTargets.Method)]
public sealed class SubscribeAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)]
public sealed class SubscribeAttachmentAttribute : Attribute { }