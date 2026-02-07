using BB.Di;
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
    public abstract class BaseEntitySpawnManager<TContext> : EntitySystem where TContext : ISpawnContext
    {
        readonly Dictionary<IEntityInstaller, EntitySpawnData> _spawnDatas = new();
        protected WorldSetup World => Entity._ref.World;
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
            => new EntityFactory(World, pool, injector, context.Installer);
        protected IEntity GetUnspawnedEntity(in TContext context)
        {
            var data = GetData(context);

            var parentEntity = context.Parent?._ref ?? World.ParentEntity;
            if (!data.Pool.TryGetEntity(out var entity))
            {
                entity = data.Factory.Create(new()
                {
                    Name = $"{data.Installer.Name} {data.AllEntities.Count + 1}",
                });
                data.AllEntities.Add(entity);
            }
            entity.Parent = parentEntity;
            InitEntityBeforeInjection(entity, context);
            entity.Inject();
            return entity;
        }
        protected virtual void InitEntityBeforeInjection(IEntity entity, in TContext context)
        {
        }
        protected IEntityInjector CreateInjector(in TContext context)
        {
            return new EntityInjector(context.Installer, World);
        }
        private EntitySpawnData GetData(in TContext context)
        {
            if (_spawnDatas.TryGetValue(context.Installer, out var data))
                return data;

            data = CreateNewData(context);
            _spawnDatas.Add(context.Installer, data);
            return data;
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
    public interface IEntityPool
    {
        bool TryGetEntity(out IEntity entity);
        void ReturnEntity(IEntity entity);
    }
}