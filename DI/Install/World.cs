using System;
using System.Collections.Generic;
using UnityEngine;
using BB.Di;
namespace BB
{
    public static partial class World
    {
        static readonly List<EntityImpl> _entities = new();
        static EntityImpl TopEntity => _entities.Count > 0 ? _entities[^1] : null;

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
                null,
                true);
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

        public static Entity Spawn(IEntityInstaller installer)
            => installer.Spawn(Entity);
        public static T Require<T>()
        {
            if (!Has(out T result))
            {
                using var _ = Log.Logger.UseContext(Entity);
                throw new DiException($"World does not have {typeof(T).Name} bound.");
            }
            return result;
        }
        public static void Require<T1, T2>(out T1 t1, out T2 t2)
        {
            t1 = Require<T1>();
            t2 = Require<T2>();
        }
        public static void Require<T1, T2, T3>(out T1 t1, out T2 t2, out T3 t3)
        {
            t1 = Require<T1>();
            t2 = Require<T2>();
            t3 = Require<T3>();
        }
        public static void Require<T1, T2, T3, T4>(out T1 t1, out T2 t2, out T3 t3, out T4 t4)
        {
            t1 = Require<T1>();
            t2 = Require<T2>();
            t3 = Require<T3>();
            t4 = Require<T4>();
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
        public static IEntity EntityRef
            => Application.isPlaying ? TopEntity : null;//EditorWorld.Entity;
        public static Entity Entity => EntityRef.GetToken();
        public static void Publish<T>(T msg = default) => Entity.Publish(msg);
        public static bool Has<T>(out T system) => EntityRef.Has(out system);
        public static bool Has<T1, T2>(out T1 t1, out T2 t2) => Entity.Has(out t1, out t2);
        public static bool Has<T1, T2, T3>(out T1 t1, out T2 t2, out T3 t3)
            => Entity.Has(out t1, out t2, out t3);
    }
}