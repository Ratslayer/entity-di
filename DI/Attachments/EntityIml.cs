using System;
using System.Collections.Generic;
namespace BB.Di
{
	public static class EntityAttachmentExtensions
	{
		public static void AttachTo(this Entity entity, Entity target)
		{
			if (!entity)
				return;
			if (target)
				(entity._ref as EntityImpl).AttachTo(target._ref);
			else (entity._ref as EntityImpl).Detach();
		}
		public static void Detach(this Entity entity)
		{
			if (entity)
				(entity._ref as EntityImpl).Detach();
		}
		public static void AddOnDespawn(this Entity entity, Action action)
		{
			if (entity)
				(entity._ref as EntityImpl).ExternalDespawned += action;
		}
		public static void RemoveOnDespawn(this Entity entity, Action action)
		{
			if (entity)
				(entity._ref as EntityImpl).ExternalDespawned -= action;
		}
	}
	public readonly struct EntityExternalDespawnToken : IDisposable
	{
		readonly Entity _entity;
		readonly Action _action;
		public EntityExternalDespawnToken(Entity entity, Action action)
		{
			_entity = entity;
			_action = action;
		}

		public void Dispose()
		{
			_entity.RemoveOnDespawn(_action);
		}
	}
	public sealed partial class EntityImpl : IEntity
	{
		readonly List<IExternalSubscription> _attachedSubscriptions = new();
		public IEntity AttachedToEntity => _attachedTo;
		EntityImpl _attachedTo;
		IExternalSubscription _attachedSubscription;
		event Action DespawnedExternal;
		bool IsAttachedInHierarchy(out IEntity attachedTo)
		{
			attachedTo = _attachedTo;
			if (attachedTo is not null)
				return true;
			if (_parent is not null)
				return _parent.IsAttachedInHierarchy(out attachedTo);
			return false;
		}
		public void AttachTo(IEntity entity)
		{
			DetachFromCurrentEntity();
			Attach(this);
			if (_children is not null)
				foreach (var child in _children)
					Attach(child);
			void Attach(EntityImpl e)
			{
				e._attachedTo = entity as EntityImpl;
				e._attachedSubscription = new OnDespawnExternalSubscription(DetachFromCurrentEntity);
				e._attachedTo.AddExternalSubscription(e._attachedSubscription);
				SubscribeExternals();
				e.AttachEvent?.Invoke();
			}
		}
		public void Detach() => DetachFromCurrentEntity();
		partial void DetachFromCurrentEntity()
		{
			if (_attachedTo is null)
				return;
			_attachedTo.RemoveExternalSubscription(_attachedSubscription);
			UnsubscribeExternals();
			_attachedTo = null;
			_attachedSubscription = null;
		}

		partial void SubscribeExternals()
		{
			if (_attachedTo is null)
				return;
			foreach (var subscription in _externalSubscriptions)
				_attachedTo.AddExternalSubscription(subscription);
		}
		partial void UnsubscribeExternals()
		{
			if (_attachedTo is null)
				return;
			foreach (var subscription in _externalSubscriptions)
				_attachedTo.RemoveExternalSubscription(subscription);
		}
		partial void ClearExternalSubscriptions()
		{
			DespawnedExternal = null;
			_externalSubscriptions.Clear();
		}
		public void AddExternalSubscription(IExternalSubscription subscription)
		{
			_attachedSubscriptions.Add(subscription);
			subscription.Subscribe(this);
		}
		public void RemoveExternalSubscription(IExternalSubscription subscription)
		{
			_attachedSubscriptions.Remove(subscription);
			subscription.Unsubscribe(this);
		}
	}
}