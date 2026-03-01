using System;
using System.Collections.Generic;
using System.Linq;
namespace BB.Di
{
    public interface IEntityInjector
    {
        IReadOnlyDictionary<Type, IDiComponent> Components { get; }
        void InjectEntity(in PrepareEntityForSpawnContext context);
    }
    public static class IEntityInjectorExtensions
    {
        public static void InjectEntityAfterCreate(this IEntityInjector injector, IEntity entity)
        {
            var list = PooledList<IDiComponent>.GetPooled();
            list.AddRange(injector.Components.Values
                .Where(c => !c.Lazy));
            injector.InjectEntity(new()
            {
                Entity = entity,
                ComponentsToBeInjected = list
            });
            list.Dispose();
        }
        public static void InjectEntityBeforeSpawn(this IEntityInjector injector, IEntity entity)
        {
            var list = PooledList<IDiComponent>.GetPooled();
            list.AddRange(injector.Components.Values
                .Where(c => c.Dynamic));
            injector.InjectEntity(new()
            {
                Entity = entity,
                ComponentsToBeInjected = list
            });
            list.Dispose();
        }
        public static void InjectSingleEntityComponent(
            this IEntityInjector injector, IEntity entity, IDiComponent component)
        {
            var list = PooledList<IDiComponent>.GetPooled();
            list.Add(component);
            injector.InjectEntity(new()
            {
                Entity = entity,
                ComponentsToBeInjected = list
            });
            list.Dispose();
        }
    }
}