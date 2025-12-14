using System;
using System.Reflection;
using UnityEngine.XR;
namespace BB.Di
{
	public sealed class FieldElementInjector : BaseElementInjector
    {
        public FieldInfo Field { get; init; }

        public override Type InjectedType => Field.FieldType;
		protected override MemberInfo Member => Field;

        public override void Inject(in ElementInjectContext context)
        {
            Field.SetValue(context.InjectedInstance, context.ElementValue);
        }
    }
}