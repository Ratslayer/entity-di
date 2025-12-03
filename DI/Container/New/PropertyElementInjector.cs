using System;
using System.Reflection;
namespace BB.Di
{
	public sealed class PropertyElementInjector : BaseElementInjector
    {
        public PropertyInfo Property { get; init; }

        public override Type InjectedType => Property.PropertyType;

        public override void Inject(in ElementInjectContext context)
        {
            var injectedValue = GetInjectedValue(context);
            Property.SetValue(context.Instance, injectedValue);
        }
    }
}