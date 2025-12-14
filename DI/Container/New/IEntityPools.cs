using BB.Di;
using System;
using System.Collections.Generic;
namespace BB
{
    public readonly partial struct Entity
    {
        public static Entity Spawn(in SpawnEntityContext context)
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
    }
    public interface ISpawnContext
    {
        IEntityInstaller Installer { get; }
        Entity? Parent { get; }
    }
    public abstract class BaseEntitySpawnManager<TContext> where TContext : ISpawnContext
    {
        readonly Dictionary<IEntityInstaller, EntitySpawnData> _spawnDatas = new();

        protected EntitySpawnData CreateNewData(in TContext context)
        {
            var pool = new EntityPool();
            var injector = CreateInjector(context);
            var factory = CreateFactory(context, pool, injector);
            return new EntitySpawnData
            {
                Pool = pool,
                Injector = injector,
                Installer = context.Installer,
                Factory = factory
            };
        }
        protected virtual IEntityFactory CreateFactory(
            in TContext context,
            IEntityPool pool,
            IEntityInjector injector)
            => new EntityFactory(pool, injector, context.Installer);
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
            if (!data.Pool.TryGetEntity(out var entity))
            {
                entity = data.Factory.Create(new()
                {
                    Name = $"{data.Installer.Name} {data.AllEntities.Count + 1}",
                });
                data.AllEntities.Add(entity);
                entity.Parent = parentEntity;
                data.Injector.InjectEntityAfterCreate(entity);
            }
            else
            {
                entity.Parent = parentEntity;
                data.Injector.InjectEntityBeforeSpawn(entity);
            }
            return entity;
        }
        protected class EntitySpawnData
        {
            public List<IEntity> AllEntities { get; private set; } = new();
            public IEntityInstaller Installer { get; init; }
            public IEntityPool Pool { get; init; }
            public IEntityFactory Factory { get; init; }
            public IEntityInjector Injector { get; init; }
        }
    }

    public sealed class EntityInjector : IEntityInjector, IDiContainer
    {
        readonly Dictionary<Type, IDiComponent> _components = new();
        readonly List<IDiComponent> _dynamicComponents = new();
        readonly IEntityInstaller _installer;
        public EntityInjector(IEntityInstaller installer, in InitInjectorContext context)
        {
            _installer = installer;
            WorldBootstrap.Setup.BaseInstaller.Install(this);
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
            var components = context.ComponentsToBeInjected;
            if (components is null)
                return;
            var entity = (IFullEntity)context.Entity;
            for (var i = 0; i < components.Count; i++)
            {
                var component = components[i];

                var componentData = entity.GetComponentData(new()
                {
                    ContractType = component.ContractType,
                    Init = true
                });

                var elements = context.DynamicOnly ? component.DynamicElements : component.Elements;
                if (elements is null)
                    continue;

                foreach (var element in elements)
                {
                    var elementEntity = (IFullEntity)(element.Source switch
                    {
                        InjectionSource.Game => World.GetGameEntity()._ref,
                        InjectionSource.World => World.GetWorldEntity()._ref,
                        _ => entity
                    });

                    var elementData = GetInjectedValue(element.Injector.InjectedType);

                    element.Injector.Inject(new ElementInjectContext
                    {
                        Entity = elementEntity,
                        ElementValue = elementData,
                        InjectedInstance = componentData.Instance,
                        Source = element.Source
                    });

                    object GetInjectedValue(Type injectedType)
                    {
                        if (component.AdditionalParams?.Length > 0)
                        {
                            foreach (var (type, value) in component.AdditionalParams)
                            {
                                if (type == injectedType)
                                    return value;
                            }
                        }

                        var data = elementEntity.GetComponentData(new()
                        {
                            ContractType = injectedType,
                            RequestingType = component.InstanceType
                        });
                        if (data.Init())
                            components.Add(data.FactoryComponent);

                        return data.Instance;
                    }
                }
            }
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