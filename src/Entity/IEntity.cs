using EntityDi.Container;
using System;
using System.Collections.Generic;

namespace EntityDi;
public readonly struct CreatedEvent { }
public readonly struct PreSpawnedEvent { }
public readonly struct SpawnedEvent { }
public readonly struct DespawnedEvent { }
public readonly struct PostDespawnedEvent { }
public interface IEntityFactory
{
	string Name { get; }
	IEntity Create(string name, IEntity parent);
}
public interface IEntitySingleComponent
{
	void Despawn();
}
public interface IEntity
{
	string Name { get; }
	IResolver Resolver { get; }
	void AddSubscription(ISubscription subscription);
	void AppendExplicit(Type type, params (Type, object)[] args);
	void Spawn();
	void Despawn();
	bool IsSpawned { get; }
	void AddComponent(IEntitySingleComponent component);
	void RemoveComponent(IEntitySingleComponent component);
}
public sealed record Entity(string Name, IResolver Resolver) : IEntity
{
	sealed record AppendedSystem(Type Type, (Type, object)[] Args);
	readonly List<AppendedSystem> _appends = new();
	readonly List<ISubscription> _subscriptions = new();
	readonly List<IEntitySingleComponent> _components = new();
	IPublisher<CreatedEvent> _created;
	IPublisher<PreSpawnedEvent> _preSpawned;
	IPublisher<SpawnedEvent> _spawned;
	IPublisher<DespawnedEvent> _despawned;
	IPublisher<PostDespawnedEvent> _postDespawned;
	bool _initialized;
	public override string ToString() => Name;
	public bool IsSpawned { get; private set; }

	[Inject]
	void Init(
		IPublisher<CreatedEvent> created,
		IPublisher<PreSpawnedEvent> preSpawned,
		IPublisher<SpawnedEvent> spawned,
		IPublisher<DespawnedEvent> despawned,
		IPublisher<PostDespawnedEvent> postDespawned)
	{
		_created = created;
		_preSpawned = preSpawned;
		_spawned = spawned;
		_despawned = despawned;
		_postDespawned = postDespawned;
	}
	public void AddSubscription(ISubscription subscription)
	{
		_subscriptions.Add(subscription);
		if (!_initialized)
			return;
		subscription.Init();
		if (IsSpawned)
			subscription.Subscribe();
	}

	public void Despawn()
	{
		IsSpawned = false;
		foreach (var comp in _components)
			comp.Despawn();
		_components.Clear();
		_despawned.Publish();
		_postDespawned.Publish();
		foreach (var subscription in _subscriptions)
			subscription.Unsubscribe();
	}

	public void Spawn()
	{

		if (!_initialized)
		{
			_initialized = true;
			foreach (var append in _appends)
				AppendExplicit(append.Type, append.Args);
			foreach (var subscription in _subscriptions)
				subscription.Init();
			Subscribe();
			_created.Publish();
		}
		else Subscribe();
		IsSpawned = true;
		_preSpawned.Publish();
		_spawned.Publish();
		void Subscribe()
		{
			foreach (var subscription in _subscriptions)
				subscription.Subscribe();
		}
	}
	public void AppendExplicit(Type type, (Type, object)[] args)
	{
		if (Resolver.Installed)
			AppendInternal(type, args);
		else _appends.Add(new(type, args));
	}
	void AppendInternal(Type type, (Type, object)[] args)
	{
		Resolver.Create(type, args);
	}
	public void AddComponent(IEntitySingleComponent component)
	{
		_components.Add(component);
	}
	public void RemoveComponent(IEntitySingleComponent component)
	{
		_components.Remove(component);
	}
}
