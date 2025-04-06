using System;
using System.Collections.Generic;
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
		Disposed = 3
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
	}
	public interface IEntityProvider
	{
		Entity Entity { get; }
	}
	public interface IEntityDetails : IEntity
	{
		IEnumerable<(Type, object)> GetElements();
		bool Installed { get; }
	}
}