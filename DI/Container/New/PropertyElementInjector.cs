using System;
using System.Reflection;
namespace BB.Di
{
    public sealed class PropertyElementInjector : BaseInjectAttributeInjector
    {
        public PropertyInfo Property { get; init; }

        public override Type InjectedType => Property.PropertyType;
        protected override MemberInfo Member => Property;

        public override void Inject(in ElementInjectContext context)
        {
            Property.SetValue(context.InjectionTarget, context.InjectedValue);
        }
    }
}