using System;

namespace BB.Di
{
	public interface IEntitySubscription
	{
		void Subscribe(IEntity entity);
		void Unsubscribe(IEntity entity);
	}
}
