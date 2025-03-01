namespace BB.Di
{
	public interface IEntitySubscription { }
	public interface IOnSpawn : IEntitySubscription
	{
		void OnSpawn();
	}
	public interface IOnDespawn : IEntitySubscription
	{
		void OnDespawn();
	}
}
