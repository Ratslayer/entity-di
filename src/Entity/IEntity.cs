using EntityDi.Container;
using System;
using System.Collections.Generic;
namespace EntityDi;
public static class EntitySubscriptionExtensions
{
	//public static void Subscribe<EventType>(this IEntity entity, Action<EventType> action, bool removeOnDespawn)
	//{
	//	var subscription = new AttachedEventSubscription<EventType>(action);
	//	subscription.Init(entity);
	//	entity.AddSubscription(subscription, removeOnDespawn);
	//}
	//public static IEntity CreateSubEntity(this IEntity entity, string name, Action<IEntity> install)
	//{
	//	var subEntity = EntityCreationUtils.CreateEntity(name, entity, install);
	//	entity.AddSubscription(subEntity, false);
	//	return subEntity;
	//}
}
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
public interface IEntitySub
{
}
public interface IOnInit : IEntitySub
{
	void Init(IEntity entity);
}
public interface IOnSpawn : IEntitySub
{
	void Spawn();
}
public interface IOnDespawn : IEntitySub
{
	void Despawn();
}
public interface IEntity : IOnSpawn, IOnDespawn
{
	string Name { get; }
	IResolver Resolver { get; }
	void AddSubscription(IEntitySub subscription, bool removeOnDespawn = true);
	void RemoveSubscription(IEntitySub subscription);
	//void AddDespawnable(IOnDespawn disposable);
	//void RemoveDespawnable(IOnDespawn disposable);
	void AddAttachment(IAttachedEvent attachment);
	void AttachTo(IEntity entity);
	void AppendExplicit(Type type, params (Type, object)[] args);
	bool IsSpawned { get; }

}
public sealed record Entity(string Name, IResolver Resolver) : IEntity
{
	sealed record AppendedSystem(Type Type, (Type, object)[] Args) : IOnInit
	{
		public void Init(IEntity entity)
		{
			entity.AppendExplicit(Type, Args);
		}
	}
	//readonly List<AppendedSystem> _appends = new();
	readonly List<IEntitySub> _subscriptions = new();
	readonly List<IEntitySub> _tempSubscriptions = new();
	readonly List<IAttachedEvent> _attachments = new();
	//readonly List<IEntityChild> _components = new();
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
	public void AddSubscription(IEntitySub subscription, bool removeOnDespawn)
	{
		var subs = removeOnDespawn ? _tempSubscriptions : _subscriptions;
		subs.Add(subscription);
		if (!_initialized)
			return;
		if (subscription is IOnInit init)
			init.Init(this);
		if (IsSpawned && subscription is IOnSpawn s)
			s.Spawn();
	}

	public void Despawn()
	{
		if (!IsSpawned)
			return;
		IsSpawned = false;
		//foreach (var comp in _components)
		//	comp.Despawn();
		//_components.Clear();
		_despawned.Publish();
		_postDespawned.Publish();
		foreach (var subscription in _subscriptions)
			if (subscription is IOnDespawn d)
				d.Despawn();
		foreach (var subscription in _tempSubscriptions)
			if (subscription is IOnDespawn d)
				d.Despawn();
		_tempSubscriptions.Clear();
	}

	public void Spawn()
	{
		if (IsSpawned)
			return;
		if (!_initialized)
		{
			_initialized = true;
			foreach (var subscription in _subscriptions)
				if (subscription is IOnInit init)
					init.Init(this);
			foreach (var subscription in _tempSubscriptions)
				if (subscription is IOnInit init)
					init.Init(this);
			//foreach (var append in _appends)
			//	AppendExplicit(append.Type, append.Args);
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
				if (subscription is IOnSpawn s)
					s.Spawn();
			foreach (var subscription in _tempSubscriptions)
				if (subscription is IOnSpawn s)
					s.Spawn();
		}
	}
	public void AppendExplicit(Type type, (Type, object)[] args)
	{
		if (Resolver.Installed)
			AppendInternal(type, args);
		else _tempSubscriptions.Add(new AppendedSystem(type, args));
	}
	void AppendInternal(Type type, (Type, object)[] args)
	{
		Resolver.Create(type, args);
	}

	public void RemoveSubscription(IEntitySub subscription)
	{
		_subscriptions.Remove(subscription);
	}

	public void AddAttachment(IAttachedEvent attachment)
	{
		_attachments.Add(attachment);
	}

	public void AttachTo(IEntity entity)
	{
		foreach (var attachment in _attachments)
			attachment.AttachTo(entity);
		entity.AddSubscription(entity);
	}

	//public void AddDespawnable(IOnDespawn disposable)
	//{
	//	_tempSubscriptions.Add(disposable);
	//}

	//public void RemoveDespawnable(IOnDespawn disposable)
	//{
	//	_tempSubscriptions.Remove(disposable);
	//}
	//public void AddChild(IEntityChild component)
	//{
	//	_components.Add(component);
	//}
	//public void RemoveChild(IEntityChild component)
	//{
	//	_components.Remove(component);
	//}

	//public void AttachTo(IEntity entity)
	//{
	//	entity.AddChild(this);
	//	foreach (var sub in _tempSubscriptions)
	//		sub.Subscribe(entity);
	//	_attached.Publish(new AttachedEvent(entity));
	//}

	//public void AddAttachedSubscription(IAttachedSubscription subscription)
	//	=> _tempSubscriptions.Add(subscription);
}
