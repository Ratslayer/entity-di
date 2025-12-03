using System;
using System.Reflection;
namespace BB.Di
{
	public sealed class FieldElementInjector : BaseElementInjector
    {
        public FieldInfo Field { get; init; }

        public override Type InjectedType => Field.FieldType;

        public override void Inject(in ElementInjectContext context)
        {
            var injectedValue = GetInjectedValue(context);
            Field.SetValue(context.Instance, injectedValue);
        }
    }
}