using System;
using System.Collections.Generic;
using System.Linq;
namespace BB.Di
{
    public interface IDiContainer
    {
        void BindStrategy(IDiComponent component);
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
    public sealed record EntityFactory(IEntityPool Pool, IEntityInjector Injector) : IEntityFactory
    {
        public IEntity Create(in CreateEntityContext context)
        {
            return new EntityV1(context.Name, Pool, Injector.Components);
        }
    }
    public abstract class BaseEntityFactory : IEntityFactory, IDiContainer
    {
        public static readonly HashSet<Type> _forcedDynamicTypes = new();
        readonly Dictionary<Type, IDiComponent> _components = new();
        readonly List<IDiComponent> _dynamicComponents = new();
        protected readonly IEntityPool _pool;
        protected BaseEntityFactory(IEntityPool pool)
        {
            _pool = pool;
            pool.Installer.Install(this);

            var initContext = new InitDiComponentContext()
            {
                InstallerComponents = _components,
                ForcedDynamicTypes = _forcedDynamicTypes,
                WorldComponents = _worldFactory?._components,
                GameComponents = _gameFactory?._components
            };
            foreach (var component in _components.Values)
                component.Init(initContext);

            foreach (var component in _components.Values)
                if (component.Dynamic)
                    _dynamicComponents.Add(component);
        }

        public void BindStrategy(IDiComponent component)
        {
            if (!component.Validate())
                return;

            if (_components.ContainsKey(component.ContractType))
                Log.Error(
                    $"Entity installer {_pool.Installer.Name} " +
                    $"binds multiple components to {component.ContractType}");

            _components[component.ContractType] = component;
        }

        public IEntity Create(in CreateEntityContext context)
        {
            var entity = CreateEntity(context);
            entity.Init(_components.Values);
            return entity;
        }
        protected abstract BaseEntity CreateEntity(in CreateEntityContext context);

        public void PrepareEntityForSpawn(in PrepareEntityForSpawnContext context)
        {
            IReadOnlyCollection<IDiComponent> components
                = !context.FirstTime
                ? _dynamicComponents
                : _components.Values;
            if (components is null)
                return;
            var injectionContext = new DiComponentInjectContext
            {
                Entity = context.Entity
            };
            foreach (var component in components)
                component.Inject(injectionContext);

            if (context.FirstTime)
                context.Entity.Publish(new CreatedEvent());
        }

        #region Cache
        static EntityFactory _worldFactory, _gameFactory;
        static readonly Dictionary<IEntityInstaller, BaseEntityFactory> _factories = new();
        public static TFactory Get<TFactory>(IEntityPool pool, Func<IEntityPool, TFactory> create)
            where TFactory : BaseEntityFactory
        {
            if (_factories.TryGetValue(pool.Installer, out var factory))
                return (TFactory)factory;

            var newFactory = create(pool);
            _factories.Add(pool.Installer, newFactory);
            return newFactory;
        }
        #endregion
    }
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
    public interface IDiComponent
    {
        Type ContractType { get; }
        Type InstanceType { get; }
        bool Lazy { get; }
        bool Dynamic { get; }
        bool Validate();
        void Init(in InitDiComponentContext context);
        object Create(in DiComponentCreateContext context);
        void Inject(in DiComponentInjectContext context);
    }
    public readonly struct DiComponentContext
    {
        public Type ContractType { get; init; }
        public Type InstanceType { get; init; }
        public bool Lazy { get; init; }
        public (Type, object)[] AdditionalParams { get; init; }
    }
    public readonly struct InitDiComponentContext
    {
        public IReadOnlyDictionary<Type, IDiComponent> InstallerComponents { get; init; }
        public IReadOnlyCollection<Type> ForcedDynamicTypes { get; init; }
        public IReadOnlyDictionary<Type, IDiComponent> WorldComponents { get; init; }
        public IReadOnlyDictionary<Type, IDiComponent> GameComponents { get; init; }
    }
    public abstract class BaseDiComponent : IDiComponent
    {
        public Type ContractType { get; private set; }
        public Type InstanceType { get; private set; }
        public TypeInjector Injector { get; private set; }
        public bool Lazy { get; private set; }
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
            Injector = TypeInjector.Get(InstanceType);
        }

        public abstract object Create(in DiComponentCreateContext context);

        public void Inject(in DiComponentInjectContext context)
        {
            var elements = context.DynamicOnly ? DynamicElements : Elements;
            if (elements is null)
                return;
            foreach (var element in elements)
            {
                var entity = element.Source switch
                {
                    InjectionSource.Self => context.Entity,
                    InjectionSource.Game => World.GetGameEntity()._ref,
                    InjectionSource.World => World.GetWorldEntity()._ref,
                    _ => context.Entity.Parent
                };
                element.Injector.Inject(new ElementInjectContext
                {
                    Source = element.Source,
                    Entity = entity,
                    Instance = context.Instance,
                    AdditionalParams = AdditionalParams
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
                if (context.InstallerComponents.ContainsKey(injector.InjectedType))
                    source = InjectionSource.Self;
                else if (context.ForcedDynamicTypes.Contains(injector.InjectedType))
                    source = InjectionSource.Parent;
                else if (context.GameComponents?.ContainsKey(injector.InjectedType) is true)
                    source = InjectionSource.Game;
                else if (context.WorldComponents?.ContainsKey(injector.InjectedType) is true)
                    source = InjectionSource.World;
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
    }
    public sealed class InstanceDiComponent : BaseDiComponent
    {
        public InstanceDiComponent(in DiComponentContext context, object instance) : base(context)
        {
            Instance = instance;
        }

        public object Instance { get; private set; }

        public override object Create(in DiComponentCreateContext context)
            => Instance;

        public override bool Validate(IEntityInstaller installer)
        {
            if (Instance is null)
            {
                LogError(installer, "attempted to bind null instance");
                return false;
            }
            return true;
        }
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