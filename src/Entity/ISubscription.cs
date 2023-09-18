using EntityDi.Container;
using System;

namespace EntityDi;

public interface ISubscription
{
	void Init();
	void Subscribe();
	void Unsubscribe();
}
public sealed record EventSubscription<T>(Action<T> Action, IEntity Entity) : ISubscription
{
	IPublisher<T> _subscriber;
	public void Init() => _subscriber = Entity.Resolve<IPublisher<T>>();

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