using System;
using System.Collections.Generic;
using UnityEngine;
using BB.Di;
namespace BB
{
	public static class World
	{
		static readonly List<EntityImpl> _entities = new();
		static EntityImpl TopEntity => _entities.Count > 0 ? _entities[^1] : null;
		public static Entity Entity => EntityRef.GetToken();
		public static IEntity EntityRef
			=> Application.isPlaying ? TopEntity : EditorWorld.Entity;
		public static void Init(Action<IDiContainer> install)
		{
			while (_entities.Count > 0)
				PopWorld();
			PushWorld("World", install);
		}
		public static void PushWorld(string name, Action<IDiContainer> install)
		{
			var entity = EntityImpl.CreateEntity(
				name,
				TopEntity,
				install,
				null);
			_entities.Add(entity);
			entity.State = EntityState.Enabled;
		}
		public static void PopWorldUntil(IEntity entity)
		{
			while (_entities.Count > 0)
			{
				if (_entities[^1] == entity)
					return;
				_entities[^1].Dispose();
				_entities.RemoveAt(_entities.Count - 1);
			}
		}
		public static void PopWorld()
		{
			if (_entities.Count > 0)
				_entities.RemoveAt(_entities.Count - 1);
		}
		public static void RaiseEvent<T>(T msg = default) => Entity.RaiseEvent(msg);
		public static bool Has<T>(out T system) => EntityRef.Has(out system);
		public static Entity Spawn(IEntityInstaller installer) => Entity.SpawnChild(installer);
		public static T Require<T>()
		{
			if (!Has(out T result))
			{
				using var _ = Log.Logger.UseContext(Entity);
				throw new DiException($"World does not have {typeof(T).Name} bound.");
			}
			return result;
		}
		public static T Get<T>()
		{
			Has(out T result);
			return result;
		}
		public static void DestroyAllWorldEntities()
		{
			foreach (var _ in _entities.Count)
				PopWorld();
		}
	}
}