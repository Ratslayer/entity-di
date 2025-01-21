namespace BB.Di
{
	public interface IExternalSubscription
	{
		void Subscribe(IEntity entity);
		void Unsubscribe(IEntity entity);
	}
}
