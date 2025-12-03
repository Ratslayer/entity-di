using System;
namespace BB.Di
{
	public sealed class EventSubscription<TEvent> : ProtectedPooledObject<EventSubscription<TEvent>>, ISubscription
    {
        IEvent<TEvent> _event;
        Action<TEvent> _action;
        public static EventSubscription<TEvent> GetPooled(IEvent<TEvent> @event, Action<TEvent> action)
        {
            var result = GetPooledInternal();
            result._event = @event;
            result._action = action;
            return result;
        }

        public void Subscribe()
        {
            _event?.Subscribe(_action);
        }

        public void Unsubscribe()
        {
            _event?.Unsubscribe(_action);
        }
        public override void Dispose()
        {
            _event = null;
            _action = null;
            base.Dispose();
        }
    }
}