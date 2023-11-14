using EntityDi.Container;
using System;

namespace EntityDi;
public static class EntityCreationUtils
{
	public static IResolver CreateResolver(string name, IResolver parent)
	{
		return new DiContainer(name, (DiContainer)parent);
	}
	public static IEntity CreateEntity(string name, IEntity parent, Action<IEntity> bind)
	{
		parent ??= World.Entity;
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
		entity.Event<PreSpawnedEvent>();
		entity.Event<SpawnedEvent>();
		entity.Event<DespawnedEvent>();
		entity.Event<PostDespawnedEvent>();
		entity.Event<AttachedEvent>();
	}
}