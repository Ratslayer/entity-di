using BB.Di;
using System;
using System.Collections.Generic;
namespace BB
{
    public readonly partial struct Entity
    {
        public static Entity Spawn(in EntitySpawnContext context)
        {
            var spawner = World.Require<IEntitySpawnManager>();
            var entity = spawner.Spawn(new()
            {
                Installer = context.Installer,
                Parent = context.Parent
            });

            if (!string.IsNullOrWhiteSpace(context.SerializationName))
                EntitySerializationUtils.RegisterAsSerializedEntity(entity, context.SerializationName);

            return entity;
        }
    }
}
namespace BB.Di
{

    //public interface IEntityPools
    //{
    //    IEntityPool GetPool(IEntityInstaller installer);
    //}
    public interface IEntitySpawnManager
    {
        Entity Spawn(in Context context);
        public readonly struct Context : ISpawnContext
        {
            public IEntityInstaller Installer { get; init; }
            public Entity? Parent { get; init; }
        }
    }

    public sealed class EntityPool : IEntityPool
    {
        readonly List<IEntity> _entities = new();
        public void ReturnEntity(IEntity entity)
        {
            _entities.Add(entity);
        }

        public bool TryGetEntity(out IEntity entity)
            => _entities.TryRemoveLast(out entity);
    }
    public sealed class EntitySpawnManager
        : BaseEntitySpawnManager<IEntitySpawnManager.Context>,
        IEntitySpawnManager
    {
        public Entity Spawn(in IEntitySpawnManager.Context context)
        {
            var entity = GetUnspawnedEntity(context);
            entity.SetState(EntityState.Enabled);
            return entity.GetToken();
        }

        protected override EntitySpawnData CreateNewData(in IEntitySpawnManager.Context context)
        {
            var pool = new EntityPool();
            var injector = CreateInjector(context);
            var factory = new EntityFactory(pool, injector, context.Installer);
            return new()
            {
                Pool = new EntityPool(),
                Injector = injector,
                Factory = factory,
                Installer = context.Installer,
            };
        }
    }
    public interface ISpawnContext
    {
        IEntityInstaller Installer { get; }
        Entity? Parent { get; }
    }
    public abstract class BaseEntitySpawnManager<TContext> where TContext : ISpawnContext
    {
        readonly Dictionary<IEntityInstaller, EntitySpawnData> _spawnDatas = new();

        protected abstract EntitySpawnData CreateNewData(in TContext context);
        protected IEntity GetUnspawnedEntity(in TContext context)
        {
            var data = GetData(context);
            var entity = GetUnspawnedEntity(data, context.Parent);
            return entity;
        }
        protected IEntityInjector CreateInjector(in TContext context)
        {
            return new EntityInjector(context.Installer, WorldBootstrap.Setup.GetInjectorContext());
        }
        private EntitySpawnData GetData(in TContext context)
        {
            if (_spawnDatas.TryGetValue(context.Installer, out var data))
                return data;

            data = CreateNewData(context);
            _spawnDatas.Add(context.Installer, data);
            return data;
        }
        private IEntity GetUnspawnedEntity(EntitySpawnData data, Entity? parent)
        {
            var parentEntity = parent?._ref ?? World.RootEntity;
            var firstTime = false;
            if (!data.Pool.TryGetEntity(out var entity))
            {
                entity = data.Factory.Create(new()
                {
                    Name = $"{data.Installer.Name} {data.AllEntities.Count + 1}",
                });
                data.AllEntities.Add(entity);
                firstTime = true;

            }

            entity.Parent = parentEntity;
            data.Injector.InjectEntity(new()
            {
                Entity = entity,
                Parent = parentEntity,
                FirstTime = firstTime
            });

            if (firstTime)
                entity.Publish(new EntityCreatedEvent());

            return entity;
        }
        protected sealed class EntitySpawnData
        {
            public List<IEntity> AllEntities { get; private set; } = new();
            public IEntityInstaller Installer { get; init; }
            public IEntityPool Pool { get; init; }
            public IEntityFactory Factory { get; init; }
            public IEntityInjector Injector { get; init; }
        }
    }
    public interface IEntityInjector
    {
        IReadOnlyDictionary<Type, IDiComponent> Components { get; }
        void InjectEntity(in PrepareEntityForSpawnContext context);
    }

    public sealed class EntityInjector : IEntityInjector, IDiContainer
    {
        readonly Dictionary<Type, IDiComponent> _components = new();
        readonly List<IDiComponent> _dynamicComponents = new();
        readonly IEntityInstaller _installer;
        public EntityInjector(IEntityInstaller installer, in InitInjectorContext context)
        {
            _installer = installer;
            installer.Install(this);
            var componentContext = new InitDiComponentContext
            {
                WorldComponents = context.WorldComponents,
                GameComponents = context.GameComponents,
                ForcedDynamicTypes = context.ForcedDynamicTypes,
                InstallerComponents = _components
            };

            foreach (var component in _components.Values)
                component.Init(componentContext);

            foreach (var component in _components.Values)
                if (component.Dynamic)
                    _dynamicComponents.Add(component);
        }

        public IReadOnlyDictionary<Type, IDiComponent> Components => _components;

        public void AddComponent(IDiComponent component)
        {
            if (!component.Validate(_installer))
                return;

            if (_components.ContainsKey(component.ContractType))
                Log.Error(
                    $"Entity installer {_installer.Name} " +
                    $"binds multiple components to {component.ContractType}");

            _components[component.ContractType] = component;
        }


        public void InjectEntity(in PrepareEntityForSpawnContext context)
        {
            IReadOnlyCollection<IDiComponent> components
                = !context.FirstTime
                ? _dynamicComponents
                : _components.Values;
            if (components is null)
                return;
            var injectionContext = new DiComponentInjectContext
            {
                Entity = context.Entity
            };
            foreach (var component in components)
                component.Inject(injectionContext);
        }
    }
    public interface IEntityPool
    {
        bool TryGetEntity(out IEntity entity);
        void ReturnEntity(IEntity entity);
    }
    //public sealed class EntityPools : IEntityPools
    //{
    //    readonly Dictionary<IEntityInstaller, IEntityPool> _pools = new();
    //    public IEntityPool GetPool(IEntityInstaller installer)
    //    {
    //        if (_pools.TryGetValue(installer, out IEntityPool pool))
    //            return pool;

    //        var newPool = new EntityPool(installer);
    //        _pools.Add(installer, newPool);
    //        return newPool;
    //    }
    //}
    //public abstract class BaseEntityPool<> : IEntityPool
    //{
    //    readonly EntityFactory _entityFactory;
    //    readonly List<IEntity> _entities = new(), _availableEntities = new();
    //    public IEntityInstaller Installer { get; private set; }
    //    public BaseEntityPool(IEntityInstaller installer)
    //    {
    //        Installer = installer;
    //        _entityFactory = BEF.Get(this);
    //    }
    //    protected abstract
    //    public IEntity GetUnspawnedEntity(IEntity parent)
    //    {
    //        parent ??= World.RootEntity;
    //        IEntity entity;
    //        if (_availableEntities.Count == 0)
    //        {
    //            entity = _entityFactory.Create(new()
    //            {
    //                Name = $"{Installer.Name} {_entities.Count + 1}"
    //            });

    //            _entities.Add(entity);
    //            _entityFactory.PrepareEntityForSpawn(new()
    //            {
    //                Entity = entity,
    //                Parent = parent,
    //                FirstTime = true
    //            });
    //        }
    //        else
    //        {
    //            entity = _availableEntities.RemoveLast();
    //            _entityFactory.PrepareEntityForSpawn(new()
    //            {
    //                Entity = entity,
    //                Parent = parent,
    //                FirstTime = false
    //            });
    //        }

    //        return entity;
    //    }

    //    public void ReturnEntity(IEntity entity)
    //    {
    //        _availableEntities.Add(entity);
    //    }
    //}
}