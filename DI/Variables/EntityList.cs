using BB.Di;

namespace BB
{
	public abstract record EntityList<SelfType> : ListVariable<SelfType, Entity>, IOnDespawn
		where SelfType : EntityList<SelfType>
	{
		public void RemoveAllDespawnedEntities()
		{
			foreach (var i in -Count)
				if (!this[i])
					RemoveAt(i);
		}
		public void OnDespawn() => RemoveAllDespawnedEntities();
		protected override void OnAdd(Entity e)
		{
			e.SubscribeExternal(this);
			base.OnAdd(e);
		}
		protected override void OnRemove(Entity e)
		{
			e.UnsubscribeExternal(this);
			base.OnRemove(e);
		}
	}
}