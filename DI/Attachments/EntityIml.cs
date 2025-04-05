using System.Collections.Generic;
namespace BB.Di
{
	public sealed partial class EntityImpl : IEntity
	{
		readonly List<IAttachedSubscription> _attachedSubscriptions = new();
		public IEntity AttachedToEntity => _attachedTo;
		EntityImpl _attachedTo;
		IAttachedSubscription _attachedSubscription;
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
				e._attachedSubscription 
					= DespawnExternalSubscription
					.GetPooled()
					.WithEntity(this.GetToken());
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
			_attachedSubscription.TryDispose();
			_attachedSubscription = null;

			UnsubscribeExternals();
			_attachedTo = null;
			
		}

		partial void SubscribeExternals()
		{
			if (_attachedTo is null)
				return;
			foreach (var subscription in _externalSubscriptionsOld)
				_attachedTo.AddExternalSubscription(subscription);
		}
		partial void UnsubscribeExternals()
		{
			if (_attachedTo is null)
				return;
			foreach (var subscription in _externalSubscriptionsOld)
				_attachedTo.RemoveExternalSubscription(subscription);
		}
		partial void ClearExternalSubscriptions()
		{
			_externalSubscriptionsOld.Clear();
		}
		public void AddExternalSubscription(IAttachedSubscription subscription)
		{
			_attachedSubscriptions.Add(subscription);
			subscription.Subscribe(this);
		}
		public void RemoveExternalSubscription(IAttachedSubscription subscription)
		{
			_attachedSubscriptions.Remove(subscription);
			subscription.Unsubscribe(this);
		}
	}
}