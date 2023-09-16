using System;
using System.Collections.Generic;

namespace EntityDi;
public interface IPool
{
	bool HasUnspawnedEntity(out IEntity entity);
	void AddCreatedEntity(IEntity entity);
	int NumInstances { get; }
}
public abstract record AbstractPool : IPool
{
	readonly List<IEntity> _instances = new();
	public int NumInstances => _instances.Count;
	public bool HasUnspawnedEntity(out IEntity entity)
		=> _instances.TryGet(out entity, e => !e.IsSpawned);
	public virtual void AddCreatedEntity(IEntity entity)
	{
		_instances.Add(entity);
		entity.Append<RemoveFromPoolOnDispose>(this);
	}
	sealed record RemoveFromPoolOnDispose(AbstractPool Pool)
		: EntitySystem, IDisposable
	{
		public void Dispose()
		{
			Pool._instances.Remove(Entity);
		}
	}
}
public sealed record Pool : AbstractPool;
public abstract record AbstractPools : EntitySystem
{
	readonly Dictionary<IEntityFactory, IPool> _factoryPools = new();
	protected abstract IPool CreatePool();
	protected IPool GetOrCreate<TKey>(Dictionary<TKey, IPool> pools, TKey key)
	{
		if (!pools.TryGetValue(key, out var result))
		{
			result = CreatePool();
			pools.Add(key, result);
		}
		return result;
	}
	IPool GetOrCreatePool(IEntityFactory factory)
	{
		if (factory == null)
			throw new Exception($"Can't pool a null factory.");
		return GetOrCreate(_factoryPools, factory);
	}
	public IEntity GetOrCreateUnspawnedEntity(IEntityFactory factory)
	{
		var pool = GetOrCreatePool(factory);
		if (!pool.HasUnspawnedEntity(out var entity))
		{
			entity = factory.Create($"{factory.Name} {pool.NumInstances}", Entity);
			pool.AddCreatedEntity(entity);
		}
		return entity;
	}
}