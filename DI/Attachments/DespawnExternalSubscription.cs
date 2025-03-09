using BB.Di;
namespace BB
{
	public sealed class DespawnExternalSubscription 
		: PooledObject<DespawnExternalSubscription>
		, IAttachedSubscription
	{
		Entity _entity;
		public DespawnExternalSubscription WithEntity(Entity entity)
		{
			_entity = entity;
			return this;
		}
		void Despawn() => _entity.Despawn();
		public override void Dispose()
		{
			_entity = default;
			base.Dispose();
		}
		public void Subscribe(IEntity entity)
		{
			(entity as EntityImpl).DespawnEvent += Despawn;
		}

		public void Unsubscribe(IEntity entity)
		{
			(entity as EntityImpl).DespawnEvent -= Despawn;
		}
	}
}