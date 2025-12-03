using System;
using System.Reflection;
namespace BB.Di
{
    public interface IElementInjector
    {
        Type InjectedType { get; }
        Attribute Attribute { get; }
        void Inject(in ElementInjectContext context);
    }

    public sealed class UpdateSubscriptionInjector : BaseElementInjector
    {
        public MethodInfo Method { get; init; }
        public override Type InjectedType => null;
        public override void Inject(in ElementInjectContext context)
        {
            var action = DiEventsUtils.CreateAction<UpdateTime>(Method, context.Instance, context.Entity);
            var attribute = (OnUpdateAttribute)Attribute;
            var updateType = attribute._type;
            context.Entity.AddUpdateSubscription(action, updateType);
        }
    }
}