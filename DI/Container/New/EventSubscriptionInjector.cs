using System;
using System.Reflection;
namespace BB.Di
{
    public sealed class EventSubscriptionInjector : BaseElementInjector
    {
        public MethodInfo Method { get; private set; }
        public Type EventType { get; private set; }
        public override Type InjectedType { get; }
        protected override MemberInfo Member => Method;

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
            var subscription = (ISubscription)_subscriptionCreationMethod.Invoke(null, new[]
            {
                context.ElementValue, Method, context.InjectedInstance, context.Entity
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