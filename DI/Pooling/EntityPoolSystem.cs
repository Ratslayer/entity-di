//using System;
//using System.Collections.Generic;

//public sealed record EntityPoolSystem : EntitySystem, IEntityPool, IDisposable
//{
//	sealed class Pool
//	{
//		public readonly Dictionary<object, List<IEntity>> _entities = new();
//	}
//	readonly Dictionary<IEntity, Pool> _pools = new();
//	public Entity Spawn(string name, Action<IDiContainer> install, Entity parent)
//	{
//		if (!parent)
//			parent = World.Entity;
//		var p = parent._ref is EntityImpl e ? e : World.EntityRef as EntityImpl;

//		if (!_pools.TryGetValue(p, out var pool))
//		{
//			pool = new Pool();
//			_pools.Add(p, pool);
//		}
//		if (!pool._entities.TryGetValue(install, out var spawned))
//		{
//			spawned = new();
//			pool._entities.Add(install, spawned);
//		}
//		if (!spawned.Contains(out var entity, e => !e.IsSpawned))
//		{
//			var ent = new EntityImpl($"{name} {spawned.Count + 1}", p, install);
//			ent.Container.System<RemoveFromPoolOnDispose>(spawned);
//			ent.Install();
//			entity = ent;
//			spawned.Add(entity);
//		}
//		return entity.Spawn();
//	}
//	public void Dispose()
//	{
//		foreach (var pool in _pools.Values)
//			foreach (var entities in pool._entities.Values)
//				foreach (var entity in entities)
//					entity.Dispose();
//		_pools.Clear();
//	}

//}
