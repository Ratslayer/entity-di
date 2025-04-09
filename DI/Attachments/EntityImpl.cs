namespace BB.Di
{
	public sealed partial class EntityImpl : IEntity
	{
		public IEntity AttachedToEntity => _attachedTo;
		EntityImpl _attachedTo;
		IEntitySubscription _detachOnDespawnSubscription;
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

				e._detachOnDespawnSubscription
					= DespawnExternalSubscription.GetPooled(this.GetToken());
				e._attachedTo.AddTemporarySubscription(e._detachOnDespawnSubscription);
				
				foreach(var sub in _subscriptionsOnAttach)
					entity.AddTemporarySubscription(sub);
				e.AttachEvent?.Invoke();
			}
		}
		public void Detach() => DetachFromCurrentEntity();
		partial void DetachFromCurrentEntity()
		{
			if (_attachedTo is null)
				return;

			_attachedTo.RemoveTemporarySubscription(_detachOnDespawnSubscription);
			_detachOnDespawnSubscription.TryDispose();
			_detachOnDespawnSubscription = null;

			foreach (var sub in _subscriptionsOnAttach)
				_attachedTo.RemoveTemporarySubscription(sub);
			_attachedTo = null;
			
		}
	}
}