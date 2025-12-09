using System;
namespace BB.Di
{
    public sealed class EventSubscription<TEvent>
        : ProtectedPooledObject<EventSubscription<TEvent>>, ISubscription, IEventHandler<TEvent>
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
            _event?.Subscribe(this);
        }

        public void Unsubscribe()
        {
            _event?.Unsubscribe(this);
        }
        public override void Dispose()
        {
            _event = null;
            _action = null;
            base.Dispose();
        }

        public void OnEvent(TEvent msg) => _action.Invoke(msg);
	}
}