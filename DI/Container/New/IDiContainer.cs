using System;
using System.Collections.Generic;
using System.Linq;
namespace BB.Di
{
    public interface IDiContainer
    {
        void AddComponent(IDiComponent component);
    }
    public interface IDiResolver
    {
        IReadOnlyCollection<DiComponentContext> Elements { get; }
    }
    public enum InjectionSource
    {
        Self,
        World,
        Game,
        Parent
    }

    public readonly struct ElementInjectContext
    {
        public object Instance { get; init; }
        public (Type, object)[] AdditionalParams { get; init; }
        public IEntity Entity { get; init; }
        public InjectionSource Source { get; init; }
    }
    public sealed record EntityFactory(
        IEntityPool Pool, 
        IEntityInjector Injector,
        IEntityInstaller Installer) : IEntityFactory
    {
        public IEntity Create(in CreateEntityContext context)
        {
            return new EntityV1(context.Name, Pool, Installer);
        }
    }
    //public abstract class BaseEntityFactory : IEntityFactory, IDiContainer
    //{
    //    public static readonly HashSet<Type> _forcedDynamicTypes = new();
    //    readonly Dictionary<Type, IDiComponent> _components = new();
    //    readonly List<IDiComponent> _dynamicComponents = new();
    //    readonly IEntityInstaller _installer;
    //    protected BaseEntityFactory(IEntityInstaller installer)
    //    {
    //        _installer = installer;
    //        WorldBootstrap.Setup.BaseInstaller.Install(this);
    //        _installer.Install(this);

    //        var initContext = new InitDiComponentContext()
    //        {
    //            InstallerComponents = _components,
    //            ForcedDynamicTypes = _forcedDynamicTypes,
    //            WorldComponents = _worldFactory?._components,
    //            GameComponents = _gameFactory?._components
    //        };
    //        foreach (var component in _components.Values)
    //            component.Init(initContext);

    //        foreach (var component in _components.Values)
    //            if (component.Dynamic)
    //                _dynamicComponents.Add(component);
    //    }

    //    public void AddComponent(IDiComponent component)
    //    {
    //        if (!component.Validate())
    //            return;

    //        if (_components.ContainsKey(component.ContractType))
    //            Log.Error(
    //                $"Entity installer {_installer.Name} " +
    //                $"binds multiple components to {component.ContractType}");

    //        _components[component.ContractType] = component;
    //    }

    //    public IEntity Create(in CreateEntityContext context)
    //    {
    //        var entity = CreateEntity(context);
    //        entity.Init(_components.Values);
    //        return entity;
    //    }
    //    protected abstract BaseEntity CreateEntity(in CreateEntityContext context);

    //    public void PrepareEntityForSpawn(in PrepareEntityForSpawnContext context)
    //    {
    //        IReadOnlyCollection<IDiComponent> components
    //            = !context.FirstTime
    //            ? _dynamicComponents
    //            : _components.Values;
    //        if (components is null)
    //            return;
    //        var injectionContext = new DiComponentInjectContext
    //        {
    //            Entity = context.Entity
    //        };
    //        foreach (var component in components)
    //            component.Inject(injectionContext);

    //        if (context.FirstTime)
    //            context.Entity.Publish(new EntityCreatedEvent());
    //    }

    //    //#region Cache
    //    //static EntityFactory _worldFactory, _gameFactory;
    //    //static readonly Dictionary<IEntityInstaller, BaseEntityFactory> _factories = new();
    //    //public static TFactory Get<TFactory>(IEntityPool pool, Func<IEntityPool, TFactory> create)
    //    //    where TFactory : BaseEntityFactory
    //    //{
    //    //    if (_factories.TryGetValue(pool.Installer, out var factory))
    //    //        return (TFactory)factory;

    //    //    var newFactory = create(pool);
    //    //    _factories.Add(pool.Installer, newFactory);
    //    //    return newFactory;
    //    //}
    //    //#endregion
    //}
    public readonly struct DiComponentCreateContext
    {
        public IEntity Entity { get; init; }
    }
    public readonly struct ValidateDiComponentContext
    {
        public IEntityInstaller EntityInstaller { get; init; }
    }
    public sealed class DiElement
    {
        public InjectionSource Source { get; init; }
        public IElementInjector Injector { get; init; }
    }
    public readonly struct DiComponentContext
    {
        public Type ContractType { get; init; }
        public Type InstanceType { get; init; }
        public bool Lazy { get; init; }
        public (Type, object)[] AdditionalParams { get; init; }
        public object[] TypelessAdditionalParams
        {
            init
            {
                if (value is not { Length: > 0 })
                    return;
                AdditionalParams = value
                    .Select(x => (x.GetType(), x))
                    .ToArray();
            }
        }
    }
    public readonly struct InitDiComponentContext
    {
        public IReadOnlyDictionary<Type, IDiComponent> InstallerComponents { get; init; }
        public IReadOnlyCollection<Type> ForcedDynamicTypes { get; init; }
        public IReadOnlyDictionary<Type, IDiComponent> WorldComponents { get; init; }
        public IReadOnlyDictionary<Type, IDiComponent> GameComponents { get; init; }
    }
    public readonly struct InitInjectorContext
    {
        public IReadOnlyCollection<Type> ForcedDynamicTypes { get; init; }
        public IReadOnlyDictionary<Type, IDiComponent> WorldComponents { get; init; }
        public IReadOnlyDictionary<Type, IDiComponent> GameComponents { get; init; }
    }
    public sealed class ConstructDiComponent : BaseDiComponent
    {
        public ConstructDiComponent(in DiComponentContext context) : base(context)
        {
        }

        public override object Create(in DiComponentCreateContext context)
            => Activator.CreateInstance(InstanceType);

        public override bool Validate(IEntityInstaller installer)
        {
            var constructor = InstanceType.GetConstructor(Array.Empty<Type>());
            if (constructor is null)
            {
                LogError(installer, $"Can't bind {InstanceType}. It has no default constructor");
                return false;
            }

            return true;
        }
    }
    public readonly struct DiComponentInjectContext
    {
        public IEntity Entity { get; init; }
        public object Instance { get; init; }
        public bool DynamicOnly { get; init; }
    }
    public interface IEntityFactory
    {
        IEntity Create(in CreateEntityContext context);
        //void PrepareEntityForSpawn(in PrepareEntityForSpawnContext context);
    }
    public readonly struct CreateEntityContext
    {
        public string Name { get; init; }
    }
    public readonly struct PrepareEntityForSpawnContext
    {
        public IEntity Entity { get; init; }
        public IEntity Parent { get; init; }
        public bool FirstTime { get; init; }
    }
}