using System;
namespace BB.Di
{
	public abstract class BaseElementInjector : IElementInjector
    {
        public Attribute Attribute { get; init; }
        public abstract Type InjectedType { get; }

        public abstract void Inject(in ElementInjectContext context);

        protected object GetInjectedValue(in ElementInjectContext context)
        {
            if (context.AdditionalParams?.Length > 0)
            {
                foreach (var (type, value) in context.AdditionalParams)
                {
                    if (type == InjectedType)
                        return value;
                }
            }

            return GetEntityComponent(context);
        }
        protected object GetEntityComponent(in ElementInjectContext context)
        {
            if (context.Entity.TryResolve(InjectedType, out var entityValue))
                return entityValue;

            throw new ArgumentNullException($"Could not resolve element of type {InjectedType.Name}");

        }
    }
}