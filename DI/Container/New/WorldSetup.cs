using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BB.Di
{
    public sealed record WorldData(IEntityFactory Factory, IEntityInjector Injector, IEntity Entity);
    public sealed class WorldSetup
    {
        public IEntityInstaller BaseInstaller { get; init; }
        public List<WorldData> Worlds { get; init; } = new();
        public Dictionary<Type, TypeInjector> Injectors { get; init; } = new();
        public Dictionary<IEntityInstaller, IEntityFactory> Factories { get; init; } = new();
        public HashSet<Type> ForcedDynamicTypes { get; init; }

        public InitInjectorContext GetInjectorContext()
            => new()
            {
                WorldComponents = Worlds.AtIndexOrDefault(0)?.Injector.Components,
                GameComponents = Worlds.AtIndexOrDefault(1)?.Injector.Components,
                ForcedDynamicTypes = ForcedDynamicTypes
            };


    }
    public interface IWorldInitializer
    {
        WorldSetupConfig Init();
    }
    public class AdditionalInstaller : IEntityInstaller
    {
        public string Name => string.Empty;

        public virtual void Install(IDiContainer container)
        {
            container.Event<EntityCreatedEvent>();
            container.Event<EntitySpawnedEvent>();
            container.Event<PostEntitySpawnedEvent>();
            container.Event<EntityEnabledEvent>();
            container.Event<EntityDisabledEvent>();
            container.Event<EntityDespawnedEvent>();
        }
    }
    public readonly struct WorldSetupConfig
    {
        public IEntityInstaller AdditionalInstaller { get; init; }
        public IEntityInstaller WorldInstaller { get; init; }
        public IEnumerable<Type> ForcedDinamicTypes { get; init; }
    }
    public static class WorldBootstrap
    {
        public static WorldSetup Setup { get; private set; }
        public static void Init()
        {
            if (Setup is not null)
                return;

            var worldSetupConfigFactoryType = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && typeof(IWorldInitializer).IsAssignableFrom(t))
                .FirstOrDefault()
                ?? throw new DiException(
                    $"Could not find any class implementing " +
                    $"{typeof(IWorldInitializer).FullName}");

            var worldSetupConfigFactory
                = (IWorldInitializer)Activator.CreateInstance(worldSetupConfigFactoryType);

            var worldSetupConfig = worldSetupConfigFactory.Init();

            Setup = new WorldSetup
            {
                BaseInstaller = worldSetupConfig.AdditionalInstaller,
                ForcedDynamicTypes = worldSetupConfig.ForcedDinamicTypes.ToHashSet()
            };
            AddWorld("World", worldSetupConfig.WorldInstaller);
            Setup.Worlds[0].Entity.Publish(new AfterWorldSpawnEvent());
        }
        public static void AddWorld(string name, IEntityInstaller installer)
        {
            var injector = new EntityInjector(installer, Setup.GetInjectorContext());
            var factory = new EntityFactory(null, injector, installer);
            var entity = factory.Create(new() { Name = name });

            Setup.Worlds.Add(new(factory, injector, entity));

            entity.SetState(EntityState.Enabled);
        }
        public static void ClearWorldEntitiesExceptBase()
            => ClearWorldEntities(1);
        public static void ClearWorldEntities()
            => ClearWorldEntities(0);
        private static void ClearWorldEntities(int leaveNum)
        {
            while (Setup.Worlds.Count > 1)
            {
                var entity = Setup.Worlds.RemoveLast().Entity;
                entity.SetState(EntityState.Destroyed);
            }
        }
    }
}
