using EntityDi.Container;
using System;
using System.Collections.Generic;

namespace EntityDi;
public readonly struct CreatedEvent { }
public readonly struct PreSpawnedEvent { }
public readonly struct SpawnedEvent { }
public readonly struct DespawnedEvent { }
public readonly struct PostDespawnedEvent { }
public readonly record struct AttachedEvent(IEntity Entity);
public interface IEntityFactory
{
	string Name { get; }
	IEntity Create(string name, IEntity parent);
}
public interface IEntityChild
{
	void Despawn();
}
public interface IEntity : IEntityChild
{
	string Name { get; }
	IResolver Resolver { get; }
	void AddSubscription(ISubscription subscription);
	void AppendExplicit(Type type, params (Type, object)[] args);
	void Spawn();
	void Despawn();
	bool IsSpawned { get; }
	void AddChild(IEntityChild child);
	void RemoveChild(IEntityChild child);
	void AttachTo(IEntity entity);
	void AddAttachedSubscription(IAttachedSubscription subscription);
}
public sealed record Entity(string Name, IResolver Resolver) : IEntity
{
	sealed record AppendedSystem(Type Type, (Type, object)[] Args);
	readonly List<AppendedSystem> _appends = new();
	readonly List<ISubscription> _subscriptions = new();
	readonly List<IAttachedSubscription> _attachedSubscriptions = new();
	readonly List<IEntityChild> _components = new();
	IPublisher<CreatedEvent> _created;
	IPublisher<PreSpawnedEvent> _preSpawned;
	IPublisher<SpawnedEvent> _spawned;
	IPublisher<DespawnedEvent> _despawned;
	IPublisher<PostDespawnedEvent> _postDespawned;
	IPublisher<AttachedEvent> _attached;
	bool _initialized;
	public override string ToString() => Name;
	public bool IsSpawned { get; private set; }

	[Inject]
	void Init(
		IPublisher<CreatedEvent> created,
		IPublisher<PreSpawnedEvent> preSpawned,
		IPublisher<SpawnedEvent> spawned,
		IPublisher<DespawnedEvent> despawned,
		IPublisher<PostDespawnedEvent> postDespawned,
		IPublisher<AttachedEvent> attached)
	{
		_created = created;
		_preSpawned = preSpawned;
		_spawned = spawned;
		_despawned = despawned;
		_postDespawned = postDespawned;
		_attached = attached;
	}
	public void AddSubscription(ISubscription subscription)
	{
		_subscriptions.Add(subscription);
		if (!_initialized)
			return;
		subscription.Init(this);
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
				subscription.Init(this);
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
	public void AddChild(IEntityChild component)
	{
		_components.Add(component);
	}
	public void RemoveChild(IEntityChild component)
	{
		_components.Remove(component);
	}

	public void AttachTo(IEntity entity)
	{
		entity.AddChild(this);
		foreach (var sub in _attachedSubscriptions)
			sub.Subscribe(entity);
		_attached.Publish(new AttachedEvent(entity));
	}

	public void AddAttachedSubscription(IAttachedSubscription subscription)
		=> _attachedSubscriptions.Add(subscription);
}
