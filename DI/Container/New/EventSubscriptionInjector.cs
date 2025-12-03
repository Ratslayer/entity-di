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
            var eventComponent = GetEntityComponent(context);
            var action = DiEventsUtils.CreateAction(Method, context.Instance, context.Entity);
            var subscription = (ISubscription)_subscriptionCreationMethod
                .Invoke(null, new[] { eventComponent, action });

            context.Entity.AddSubscription(new()
            {
                Subscription = subscription,
                Source = context.Source
            });
        }
    }
}