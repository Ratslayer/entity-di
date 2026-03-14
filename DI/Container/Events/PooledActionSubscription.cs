using BB.Di;
using System;

namespace BB
{
    public sealed class PooledActionSubscription<T>
        : ProtectedPooledObject<PooledActionSubscription<T>>, IEntitySubscription, IEventHandler<T>
    {
        IEvent<T> _event;
        Action<T> _action;

        public static PooledActionSubscription<T> GetPooled(Entity entity, Action<T> action)
        {
            if (!entity.Has(out IEvent<T> e))
                return null;
            var result = GetPooledInternal();
            result._event = e;
            result._action = action;
            return result;
        }

        public void Subscribe(IEntity _ = null)
        {
            _event.Subscribe(this);
        }

        public void Unsubscribe(IEntity _ = null)
        {
            _event.Unsubscribe(this);
        }

        public override void Dispose()
        {
            Unsubscribe();
            _action = null;
            _event = null;
            base.Dispose();
        }

        public void OnEvent(T msg) => _action(msg);
    }
}