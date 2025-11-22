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
    public interface IDiResolver
    {
        IDiResolver Parent { get; set; }
        bool Locked { get; }
        void ResolveBindings();
        bool TryResolve(Type contract, out object result);
        IEnumerable<(Type, object)> GetElements();
    }
    public interface IEntity
    {
        string Name { get; }
        IEntity Parent { get; }
        ulong CurrentSpawnId { get; }
        EntityState State { get; set; }
        bool TryResolve(Type type, out object result);
        void AddSubscription(in EntitySubscriptionContext context);
        void RemoveSubscription(in EntitySubscriptionContext context);
        //IEntity CreateChild(IEntityInstaller installer);
        //void AddTemporarySubscription(IEntitySubscription subscription);
        //void RemoveTemporarySubscription(IEntitySubscription subscription);
        //void AddDespawnDisposable(IDisposable disposable);
        //void RemoveDespawnDisposable(IDisposable disposable);
        //CancellationToken DespawnCancellationToken { get; }
    }
    public readonly struct EntitySubscriptionContext
    {
        public IEntitySubscription Subscription { get; init; }
        public bool Temporary { get; init; }
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