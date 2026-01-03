using System;
using System.Reflection;
namespace BB.Di
{
    public sealed class EventSubscriptionInjector : BaseElementInjector
    {
        public MethodInfo Method { get; private set; }
        public Type EventType { get; private set; }
        public override Type InjectedType { get; }
        readonly MethodInfo _subscriptionCreationMethod;
        protected override MemberInfo Member => Method;
        public EventSubscriptionInjector(MethodInfo method, Type eventType, Attribute attribute)
        {
            Method = method;
            EventType = eventType;
            InjectedType = typeof(IEvent<>).MakeGenericType(eventType);

            var subscriptionType = typeof(EventSubscription<>).MakeGenericType(EventType);
            _subscriptionCreationMethod = subscriptionType.GetMethod(
                "GetPooled",
                BindingFlags.Public | BindingFlags.Static);
        }

        public override void Inject(in ElementInjectContext context)
        {
            var subscription = (ISubscription)_subscriptionCreationMethod.Invoke(null, new[]
            {
                context.InjectedValue, Method, context.InjectionTarget, context.Entity
            });

            context.Entity.AddSubscription(new()
            {
                Subscription = subscription,
                Source = context.Source
            });
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }
}