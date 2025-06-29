using BB.Di;
using System;

namespace BB
{
	public static class EventExtensions
	{
		public static void Publish<T>(this IEvent<T> e)
			where T : new()
			=> e.Publish(new());
		public static IEntitySubscription TempSubscribe<T>(this Entity entity, Action<T> action)
		{
			if (!entity.Has(out IEvent<T> e))
				return null;
			var subscription = PooledActionSubscription<T>.GetPooled(e, action);
			entity._ref.AddTemporarySubscription(subscription);
			return subscription;
		}
	}
	public sealed class PooledActionSubscription<T> 
		: ProtectedPooledObject<PooledActionSubscription<T>>, IEntitySubscription
	{
		IEvent<T> _event;
		Action<T> _action;
		public static PooledActionSubscription<T> GetPooled(IEvent<T> e, Action<T> action)
		{
			var result = GetPooledInternal();
			result._event = e;
			result._action = action;
			return result;
		}

		public void Subscribe(IEntity _)
		{
			_event.Subscribe(_action);
		}

		public void Unsubscribe(IEntity _)
		{
			_event.Unsubscribe(_action);
		}
	}
}