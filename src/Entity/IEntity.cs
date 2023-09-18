using EntityDi.Container;
using System;
using System.Collections.Generic;

namespace EntityDi;
public readonly struct CreatedEvent { }
public readonly struct SpawnedEvent { }
public readonly struct DespawnedEvent { }
public readonly record struct UpdatedEvent(float Delta);
public interface IEntityFactory
{
	string Name { get; }
	IEntity Create(string name, IEntity parent);
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
}
public sealed record Entity(string Name, IResolver Resolver) : IEntity
{
	sealed record AppendedSystem(Type Type, (Type, object)[] Args);
	readonly List<AppendedSystem> _appends = new();
	readonly List<ISubscription> _subscriptions = new();
	IPublisher<CreatedEvent> _created;
	IPublisher<SpawnedEvent> _spawned;
	IPublisher<DespawnedEvent> _despawned;
	bool _initialized;

	public bool IsSpawned { get; private set; }

	[Inject]
	void Init(
		IPublisher<CreatedEvent> created,
		IPublisher<SpawnedEvent> spawned,
		IPublisher<DespawnedEvent> despawned)
	{
		_created = created;
		_spawned = spawned;
		_despawned = despawned;
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
		_despawned.Publish();
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
}
