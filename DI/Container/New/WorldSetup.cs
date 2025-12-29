using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BB.Di
{
    public sealed record WorldData(IEntityFactory Factory, IEntityInjector Injector, IEntity Entity);
    public interface IWorld
    {

    }
    public sealed class WorldSetup : IDisposable, IWorld
    {
        public IEntityInstaller BaseInstaller { get; init; }
        public WorldData Core { get; set; }
        public WorldData Game { get; set; }
        public IEntity ParentEntity => (Game ?? Core)?.Entity;
        public Dictionary<IEntityInstaller, IEntityFactory> Factories { get; init; } = new();
        public HashSet<Type> ForcedDynamicTypes { get; init; }

        public InitInjectorContext GetInjectorContext()
            => new()
            {
                WorldComponents = Core?.Injector.Components,
                GameComponents = Game?.Injector.Components,
                ForcedDynamicTypes = ForcedDynamicTypes
            };
        readonly Dictionary<Type, TypeInjector> _injectors = new();
        public TypeInjector GetTypeInjector(Type type)
        {
            if (!_injectors.TryGetValue(type, out var result))
            {
                result = new TypeInjector(type);
                _injectors.Add(type, result);
            }
            if (result._errors.Count > 0)
                throw new ArgumentException(
                    $"Errors encountered during creation of {type.Name} type injector:\n" +
                    $"{string.Join('\n', result._errors)}");
            return result;
        }

        public void ClearGame()
        {
            Game?.Entity?.SetState(EntityState.Destroyed);
            Game = null;
        }
        public void ClearCore()
        {
            ClearGame();
            Core?.Entity?.SetState(EntityState.Destroyed);
            Core = null;
        }
        public void CreateCore(IEntityInstaller installer)
            => CreateWorldEntity("Core", installer, data => Core = data);
        public void CreateGame(IEntityInstaller installer)
            => CreateWorldEntity("Game", installer, data => Game = data);
        void CreateWorldEntity(string name, IEntityInstaller installer,
            Action<WorldData> processor)
        {
            var injector = new EntityInjector(installer, GetInjectorContext());
            var factory = new EntityFactory(null, injector, installer);
            var entity = factory.Create(new() { Name = name });
            entity.Parent = ParentEntity;

            processor(new(factory, injector, entity));

            injector.InjectEntityAfterCreate(entity);

            entity.SetState(EntityState.Enabled);
        }

        public void Dispose()
        {
            ClearCore();
        }
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
            container.System<EntityWrapper>();
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
        public static WorldSetup World { get; private set; }
        public static void SpawnWorld()
        {
            if (World is not null)
                return;

            var worldSetupConfig = LoadConfig();

            World = new WorldSetup
            {
                BaseInstaller = worldSetupConfig.AdditionalInstaller,
                ForcedDynamicTypes = worldSetupConfig.ForcedDinamicTypes.ToHashSet()
            };
            World.CreateCore(worldSetupConfig.WorldInstaller);
            World.Core.Entity.SetState(EntityState.Enabled);
            World.Core.Entity.Publish(new AfterWorldSpawnEvent());
        }
        public static void CreateWorld()
        {
            var worldSetupConfig = LoadConfig();
            World = new WorldSetup
            {
                BaseInstaller = worldSetupConfig.AdditionalInstaller,
                ForcedDynamicTypes = worldSetupConfig.ForcedDinamicTypes.ToHashSet()
            };
        }
        public static void DestroyWorld()
        {
            World?.Dispose();
            World = null;
        }
        static WorldSetupConfig LoadConfig()
        {
            var worldSetupConfigFactoryType = Assembly.GetExecutingAssembly()
               .GetTypes()
               .Where(t => !t.IsAbstract && typeof(IWorldInitializer).IsAssignableFrom(t))
               .FirstOrDefault()
               ?? throw new DiException(
                   $"Could not find any class implementing " +
                   $"{typeof(IWorldInitializer).FullName}");

            var worldSetupConfigFactory
                = (IWorldInitializer)Activator.CreateInstance(worldSetupConfigFactoryType);

            return worldSetupConfigFactory.Init();
        }
    }
}