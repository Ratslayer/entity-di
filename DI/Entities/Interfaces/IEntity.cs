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
        IEntity Parent { get; set; }
        ulong CurrentSpawnId { get; }
        EntityState State { get; }
        void SetState(EntityState state);
        bool TryResolve(Type type, out object result);
        void AddSubscription(in EntitySubscriptionContext context);
        void RemoveSubscription(in EntitySubscriptionContext context);
        //void AddUpdateSubscription(Action<UpdateTime> action, UpdateType type);
        //void Update(in UpdateTime time, UpdateType type);
        void AddChild(IFullEntity entity);
        void RemoveChild(IFullEntity entity);
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
        IEnumerable<(Type, object)> GetElements();
        IEnumerable<IEntity> GetChildren();
        bool Installed { get; }
        IEntityInstaller Installer { get; }
    }
}