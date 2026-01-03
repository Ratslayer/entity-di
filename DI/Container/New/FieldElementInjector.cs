using System;
using System.Reflection;
namespace BB.Di
{
	public sealed class FieldElementInjector : BaseInjectAttributeInjector
    {
        public FieldInfo Field { get; init; }

        public override Type InjectedType => Field.FieldType;
		protected override MemberInfo Member => Field;

        public override void Inject(in ElementInjectContext context)
        {
            Field.SetValue(context.InjectionTarget, context.InjectedValue);
        }
    }
}