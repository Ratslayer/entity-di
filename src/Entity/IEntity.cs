using EntityDi.Container;
namespace EntityDi;
public readonly struct CreatedEvent { }
public readonly struct SpawnedEvent { }
public readonly struct DespawnedEvent { }
public interface IEntity
{
	string Name { get; }
	IResolver Resolver { get; }
	void AddSubscription(ISubscription subscription);
	void Spawn();
	void Despawn();
}
public sealed record Entity(string Name, IResolver Resolver) : IEntity
{
	readonly List<ISubscription> _subscriptions = new();
	Event<CreatedEvent> _created;
	Event<SpawnedEvent> _spawned;
	Event<DespawnedEvent> _despawned;
	bool _initialized;
	[Inject]
	void Init(
		Event<CreatedEvent> created,
		Event<SpawnedEvent> spawned,
		Event<DespawnedEvent> despawned)
	{
		_created = created;
		_spawned = spawned;
		_despawned = despawned;
	}
	public void AddSubscription(ISubscription subscription)
	{
		_subscriptions.Add(subscription);
	}

	public void Despawn()
	{
		_despawned.Publish();
		foreach (var subscription in _subscriptions)
			subscription.Unsubscribe();
	}

	public void Spawn()
	{

		if (!_initialized)
		{
			_initialized = true;
			foreach (var subscription in _subscriptions)
				subscription.Init();
			Subscribe();
			_created.Publish();
		}
		else Subscribe();
		_spawned.Publish();
		void Subscribe()
		{
			foreach (var subscription in _subscriptions)
				subscription.Subscribe();
		}
	}

}
public static class EntityCreationUtils
{
	public static IEntity _world;
	public static IResolver CreateResolver(string name, IResolver parent)
	{
		return new DiContainer(name, (DiContainer)parent);
	}
	public static IEntity CreateEntity(string name, IEntity parent, Action<IEntity> bind)
	{
		parent = parent ?? _world;
		var resolver = CreateResolver(name, parent?.Resolver);
		var entity = new Entity(name, resolver);
		BindEntity(entity);
		bind(entity);
		resolver.Install();
		resolver.Inject(entity);
		return entity;
	}
	static void BindEntity(IEntity entity)
	{
		entity.BindInstance(entity);
		entity.Event<CreatedEvent>();
		entity.Event<SpawnedEvent>();
		entity.Event<DespawnedEvent>();
	}
}