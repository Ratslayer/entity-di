//using BB.Di;
//namespace BB
//{
//	public sealed class DespawnExternalSubscription
//		: ProtectedPooledObject<DespawnExternalSubscription>
//		, IEntitySubscription
//	{
//		Entity _entity;
//		public static DespawnExternalSubscription GetPooled(Entity entity)
//		{
//			var result = GetPooledInternal();
//			result._entity = entity;
//			return result;
//		}
//		void Despawn() => _entity.Despawn();
//		public override void Dispose()
//		{
//			_entity = default;
//			base.Dispose();
//		}
//		public void Subscribe(IEntity entity)
//		{
//			(entity as EntityImpl).DespawnEvent += Despawn;
//		}

//		public void Unsubscribe(IEntity entity)
//		{
//			(entity as EntityImpl).DespawnEvent -= Despawn;
//		}
//	}
//}