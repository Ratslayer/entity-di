using System;
namespace BB.Di
{
    public interface IElementInjector
    {
        Type InjectedType { get; }
        Attribute Attribute { get; }
        void Inject(in ElementInjectContext context);
    }
}