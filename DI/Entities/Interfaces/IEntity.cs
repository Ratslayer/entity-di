using System;
using System.Collections.Generic;
namespace BB.Di
{
    public enum EntityState
    {
        Enabled = 0,
        Disabled = 1,
        Despawned = 2,
        Destroyed = 3
    }

    public interface IEntity
    {
        string Name { get; }
        string SerializationName { get; set; }
        ulong CurrentSpawnId { get; }
        EntityState State { get; }
        WorldSetup World { get; }
        void Inject();
        void SetState(EntityState state);
        bool TryResolve(Type type, out object result);
        void AddSubscription(in EntitySubscriptionContext context);
        void RemoveSubscription(in EntitySubscriptionContext context);

        IEntity Parent { get; set; }
        IReadOnlyCollection<IEntity> Children { get; }

    }
    public interface IFullEntity : IEntity, IEntityStateHandler, IEntityDetails
    {
    }
    public readonly struct SetEntityStateContext
    {
        public EntityState State { get; init; }
    }
    public enum EntityStateEvents
    {
        //upstream
        Spawn,
        PostSpawn,
        Enable,
        //downstream
        Disable,
        Despawn
    }
    public interface IEntityStateHandler
    {
        void UpdateEffectiveState();
        void PrepareForSpawn();
        void FinalizeDespawn();
        void PublishSpawnEvent();
        void PublishPostSpawnEvent();
        void PublishEnableEvent();
        void PublishDisableEvent();
        void PublishDespawnEvent();
        void FinalizeDestroy();
    }
    public readonly struct EntitySubscriptionContext
    {
        public ISubscription Subscription { get; init; }
        public InjectionSource? Source { get; init; }
    }
    public interface IEntityProvider
    {
        Entity Entity { get; }
    }
    public interface IEntityDetails : IEntity
    {
        IReadOnlyCollection<EntityComponentData> GetElements();
        EntityComponentData GetComponentData(in GetComponentDataContext context);
        IEntityInstaller Installer { get; }
        IEntityPool Pool { get; }
    }
    public readonly struct GetComponentDataContext
    {
        public Type ContractType { get; init; }
        public Type RequestingType { get; init; }
        public bool Init { get; init; }
    }
    public readonly struct ResolvedEntityElement
    {
        public object Instance { get; init; }
        public bool NeedsInjecting { get; init; }
    }
    public readonly struct EntityElement
    {
        public Type ContractType { get; init; }
        public object Instance { get; init; }
        public IDiComponent DiComponent { get; init; }
    }
}