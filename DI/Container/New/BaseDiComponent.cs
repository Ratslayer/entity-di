using System;
using System.Collections.Generic;
using System.Linq;
namespace BB.Di
{
    public abstract class BaseDiComponent : IDiComponent
    {
        public Type ContractType { get; private set; }
        public Type InstanceType { get; private set; }
        public TypeInjector Injector { get; private set; }
        public bool Lazy { get; private set; }
        public virtual bool AlwaysCreate => false;
        public (Type, object)[] AdditionalParams { get; private set; }
        public IReadOnlyCollection<DiElement> Elements { get; private set; }
        public IReadOnlyCollection<DiElement> DynamicElements { get; private set; }
        public bool Dynamic { get; private set; }
        public BaseDiComponent(in DiComponentContext context)
        {
            ContractType = context.ContractType;
            InstanceType = context.InstanceType;
            Lazy = context.Lazy;
            AdditionalParams = context.AdditionalParams;
            Injector = context.World.GetTypeInjector(InstanceType);
        }

        public abstract object Create(IEntity entity);

        public void Inject(in DiComponentInjectContext context)
        {
            var elements = context.DynamicOnly ? DynamicElements : Elements;
            if (elements is null)
                return;
            foreach (var element in elements)
            {
                var entity = element.Source switch
                {
                    InjectionSource.Game => context.Entity.World.Game.Entity,
                    InjectionSource.Core => context.Entity.World.Core.Entity,
                    _ => context.Entity
                };

                element.Injector.Inject(new ElementInjectContext
                {
                    Entity = entity,
                    InjectionTarget = context.Instance,
                });
            }
        }

        public abstract bool Validate(IEntityInstaller installer);

        public void Init(in InitDiComponentContext context)
        {
            List<DiElement> dynamicElements = null, elements = null;
            foreach (var injector in Injector._elementInjectors)
            {
                InjectionSource source;
                if (AdditionalParams?.Contains((arg) => injector.InjectedType.IsAssignableFrom(arg.Item1)) is true)
                    source = InjectionSource.Self;
                else if (context.InstallerComponents.ContainsKey(injector.InjectedType))
                    source = InjectionSource.Self;
                else if (context.ForcedDynamicTypes.Contains(injector.InjectedType))
                    source = InjectionSource.Parent;
                else if (context.GameComponents?.ContainsKey(injector.InjectedType) is true)
                    source = InjectionSource.Game;
                else if (context.WorldComponents?.ContainsKey(injector.InjectedType) is true)
                    source = InjectionSource.Core;
                else source = InjectionSource.Parent;

                var element = new DiElement
                {
                    Source = source,
                    Injector = injector
                };
                elements ??= new();
                elements.Add(element);

                if (source is InjectionSource.Parent)
                {
                    dynamicElements ??= new();
                    dynamicElements.Add(element);
                }
            }
            Elements = elements;
            DynamicElements = dynamicElements;
            Dynamic = dynamicElements?.Count > 0;
        }
        protected void LogError(IEntityInstaller installer, string error)
        {
            Log.Error($"{installer.Name}:{ContractType.Name}: {error}");
        }
        public override string ToString()
            => $"{ContractType}:{InstanceType}";
    }
}