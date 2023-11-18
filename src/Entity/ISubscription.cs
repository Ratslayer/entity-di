using System;
using System.Collections.Generic;

namespace EntityDi;
public interface IAttachedEvent : IOnDespawn
{
	void AttachTo(IEntity entity);
}
public interface ISubscription : IOnSpawn, IOnDespawn
{
}
public sealed record Subscription<EventType>(
	ISubscriber<EventType> Subscriber,
	Action<EventType> Action)
	: ISubscription
{
	public void Spawn()
	{
		Subscriber.Subscribe(Action);
	}

	public void Despawn()
	{
		Subscriber.Unsubscribe(Action);
	}
}
public sealed record AttachedEventSubscription<T>(Action<T> Action) : IAttachedEvent
{
	readonly List<ISubscription> _subscriptions = new();

	public void Despawn()
	{
		foreach (var subscription in _subscriptions)
			subscription.Despawn();
		_subscriptions.Clear();
	}

	public void AttachTo(IEntity entity)
	{
		if (entity.TryResolve(out IPublisher<T> publisher))
		{
			var sub = new Subscription<T>((ISubscriber<T>)publisher, Action);
			_subscriptions.Add(sub);
			entity.AddComponent(sub, true);
		}
	}
}
public sealed record EventSubscription<T>(Action<T> Action) : IOnInit, IOnSpawn, IOnDespawn
{
	ISubscriber<T> _subscriber;
	public void Init(IEntity entity) => _subscriber = (ISubscriber<T>)entity.Resolve<IPublisher<T>>();

	public void Despawn()
	{
		_subscriber.Unsubscribe(Action);
	}
	public void Spawn()
	{
		_subscriber.Subscribe(Action);
	}
	public override string ToString() => $"Subscription<{typeof(T).FullName}>";
}
[AttributeUsage(AttributeTargets.Method)]
public sealed class SubscribeAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)]
public sealed class SubscribeAttachmentAttribute : Attribute { }