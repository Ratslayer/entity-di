using System;
namespace BB.Di
{
	public interface IDiComponent
    {
        Type ContractType { get; }
        Type InstanceType { get; }
        bool Lazy { get; }
        bool Dynamic { get; }
        bool Validate(IEntityInstaller installer);
        void Init(in InitDiComponentContext context);
        object Create(in DiComponentCreateContext context);
        void Inject(in DiComponentInjectContext context);
    }
}