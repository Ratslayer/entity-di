using System;
using System.Collections.Generic;
using System.Threading;
namespace BB.Di
{
	public interface IEntityAttachments
	{
		IEntity AttachedToEntity { get; }
		void AttachTo(IEntity entity);
		void Detach();
	}
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
		IEntity Parent { get; }
		ulong CurrentSpawnId { get; }
		EntityState State { get; set; }
		bool TryResolve(Type type, out object result);
		IEntity CreateChild(IEntityInstaller installer);
		void AddSubscription(IEntityEventMethod subscription);
		void RemoveSubscription(IEntityEventMethod subscription);
		void AddTemporarySubscription(IEntitySubscription subscription);
		void RemoveTemporarySubscription(IEntitySubscription subscription);
		void AddDespawnDisposable(IDisposable disposable);
		void RemoveDespawnDisposable(IDisposable disposable);
		CancellationToken DespawnCancellationToken { get; }
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
	}
}