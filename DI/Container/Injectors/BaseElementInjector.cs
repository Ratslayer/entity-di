using System;
using System.Reflection;
namespace BB.Di
{
    public abstract class BaseInjectAttributeInjector : BaseElementInjector
    {
    }
    public abstract class BaseElementInjector : IElementInjector
    {
        public Attribute Attribute { get; init; }
        protected abstract MemberInfo Member { get; }
        public abstract Type InjectedType { get; }
        public abstract void Inject(in ElementInjectContext context);
        public override string ToString()
           => $"{Member.Name}:{Attribute.GetType().Name}";
    }
}