using BB.Di;

namespace BB
{
    public sealed class PooledSubscription<T> : ProtectedPooledObject<PooledSubscription<T>>, IEntitySubscription
    {
        IEvent<T> _event;
        IEventHandler<T> _handler;

        public static PooledSubscription<T> GetPooled(Entity entity, IEventHandler<T> handler)
        {
            if (!entity.Has(out IEvent<T> e))
                return null;
            var result = GetPooledInternal();
            result._event = e;
            result._handler = handler;
            return result;
        }

        public void Subscribe(IEntity entity) => _event.Subscribe(_handler);

        public void Unsubscribe(IEntity entity) => _event.Unsubscribe(_handler);
        public override void Dispose()
        {
            _event.Unsubscribe(_handler);
            _handler = null;
            _event = null;
            base.Dispose();
        }
    }
}