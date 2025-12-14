using System;
using System.Collections.Generic;
namespace BB.Di
{
    public interface IDiComponent
    {
        Type ContractType { get; }
        Type InstanceType { get; }
        bool Lazy { get; }
        bool Dynamic { get; }
        (Type, object)[] AdditionalParams { get; }
        IReadOnlyCollection<DiElement> Elements { get; }
        IReadOnlyCollection<DiElement> DynamicElements { get; }
        bool Validate(IEntityInstaller installer);
        void Init(in InitDiComponentContext context);
        object Create(IEntity entity);
        void Inject(in DiComponentInjectContext context);
    }
}