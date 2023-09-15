using EntityDi.Container;

namespace EntityDi;

public interface ISubscription
{
	void Init();
	void Subscribe();
	void Unsubscribe();
}
public sealed record EventSubscription<T>(Action<T> Action, IEntity Entity) : ISubscription
{
	Event<T> _subscriber;
	public void Init() => _subscriber = Entity.Resolve<Event<T>>();

	public void Unsubscribe()
	{
		_subscriber.Unsubscribe(Action);
	}
	public void Subscribe()
	{
		_subscriber.Subscribe(Action);
	}
}
[AttributeUsage(AttributeTargets.Method)]
public sealed class SubscribeAttribute : Attribute { }