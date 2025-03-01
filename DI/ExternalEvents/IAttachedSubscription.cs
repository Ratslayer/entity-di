namespace BB.Di
{
	public interface IAttachedSubscription
	{
		void Subscribe(IEntity entity);
		void Unsubscribe(IEntity entity);
	}
}
