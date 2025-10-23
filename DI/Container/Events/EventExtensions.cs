using BB.Di;
using System;

namespace BB
{
	public static class EventExtensions
	{
		public static void Publish<T>(this IEvent<T> e)
			where T : new()
			=> e.Publish(new());
		public static DisposableToken TempSubscribe<T>(this Entity entity, Action<T> action)
			=> PooledActionSubscription<T>
			.GetPooled(entity, action)
			.GetToken();
    }
	public sealed class PooledActionSubscription<T> 
		: ProtectedPooledObject<PooledActionSubscription<T>>, IEntitySubscription
	{
		IEvent<T> _event;
		Action<T> _action;
		IEntity _entity;
		public static PooledActionSubscription<T> GetPooled(Entity entity, Action<T> action)
		{
			if (!entity.Has(out IEvent<T> e))
				return null;
			var result = GetPooledInternal();
			result._entity = entity._ref;
			result._event = e;
			result._action = action;
            entity._ref.AddTemporarySubscription(result);
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
		public override void Dispose()
		{
			_entity?.RemoveTemporarySubscription(this);
			_entity = null;
			_action = null;
			_event = null;
			base.Dispose();
		}
	}
}