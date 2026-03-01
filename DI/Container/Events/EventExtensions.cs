using System;

namespace BB
{
    public static class EventExtensions
    {
        public static void Publish<T>(this IEvent<T> e)
            where T : new()
            => e.Publish(new());
        public static DisposableToken Subscribe<T>(this Entity entity, Action<T> action)
        {
            var subscription = PooledActionSubscription<T>.GetPooled(entity, action);
            subscription.Subscribe();
            return subscription.GetToken();
        }
    }
}