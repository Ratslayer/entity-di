using System;
using System.Collections.Generic;
namespace BB.Di
{
	public sealed class EntityInjector : IEntityInjector, IDiContainer
    {
        readonly Dictionary<Type, IDiComponent> _components = new();
        readonly List<IDiComponent> _dynamicComponents = new();
        readonly IEntityInstaller _installer;
        public WorldSetup World { get; init; }
        public EntityInjector(IEntityInstaller installer, WorldSetup world)
        {
            _installer = installer;
            World = world;
            World.BaseInstaller.Install(this);
            installer.Install(this);

            var injectorContext = World.GetInjectorContext();
            var componentContext = new InitDiComponentContext
            {
                WorldComponents = injectorContext.WorldComponents,
                GameComponents = injectorContext.GameComponents,
                ForcedDynamicTypes = injectorContext.ForcedDynamicTypes,
                InstallerComponents = _components
            };

            foreach (var component in _components.Values)
                component.Init(componentContext);

            foreach (var component in _components.Values)
                if (component.Dynamic)
                    _dynamicComponents.Add(component);
        }

        public IReadOnlyDictionary<Type, IDiComponent> Components => _components;

        public void AddComponent(IDiComponent component)
        {
            if (!component.Validate(_installer))
                return;

            if (_components.ContainsKey(component.ContractType))
                Log.Error(
                    $"Entity installer {_installer.Name} " +
                    $"binds multiple components to {component.ContractType}");

            _components[component.ContractType] = component;
        }


        public void InjectEntity(in PrepareEntityForSpawnContext context)
        {
            var componentsToBeInjected = context.ComponentsToBeInjected;
            if (componentsToBeInjected is null)
                return;
            var entity = (IFullEntity)context.Entity;
            for (var i = 0; i < componentsToBeInjected.Count; i++)
            {
                var component = componentsToBeInjected[i];

                var componentData = entity.GetComponentData(new()
                {
                    ContractType = component.ContractType,
                    Init = true
                });

                var elements = context.DynamicOnly ? component.DynamicElements : component.Elements;
                if (elements is null)
                    continue;

                foreach (var element in elements)
                {
                    var elementEntity = (IFullEntity)(element.Source switch
                    {
                        InjectionSource.Game => World.Game.Entity,
                        InjectionSource.Core => World.Core.Entity,
                        _ => entity
                    });

                    var elementData = GetInjectedValue(element.Injector.InjectedType);

                    element.Injector.Inject(new ElementInjectContext
                    {
                        Entity = entity,
                        InjectedValue = elementData,
                        InjectionTarget = componentData.Instance,
                        Source = element.Source
                    });

                    object GetInjectedValue(Type injectedType)
                    {
                        if (component.AdditionalParams?.Length > 0)
                        {
                            foreach (var (type, value) in component.AdditionalParams)
                            {
                                if (type == injectedType)
                                    return value;
                            }
                        }

                        var data = elementEntity.GetComponentData(new()
                        {
                            ContractType = injectedType,
                            RequestingType = component.InstanceType
                        });
                        if (data.Init())
                            componentsToBeInjected.Add(data.FactoryComponent);

                        return data.Instance;
                    }
                }
            }
        }
    }
}