using System;
using System.Collections.Generic;
using System.Linq;
namespace BB.Di
{
    public interface IDiContainer
    {
        WorldSetup World { get; }
        void AddComponent(IDiComponent component);
    }
    public interface IDiResolver
    {
        IReadOnlyCollection<DiComponentContext> Elements { get; }
    }
    public enum InjectionSource
    {
        Self,
        Core,
        Game,
        Parent
    }

    public readonly struct ElementInjectContext
    {
        public object InjectionTarget { get; init; }
        public object InjectedValue { get; init; }
        public IEntity Entity { get; init; }
        public InjectionSource Source { get; init; }
    }
    public record EntityFactory(
        WorldSetup World,
        IEntityPool Pool,
        IEntityInjector Injector,
        IEntityInstaller Installer) : IEntityFactory
    {
        public virtual IEntity Create(in CreateEntityContext context)
        {
            var result = new EntityV1(context.Name, World, Pool, Injector, Installer);
            result.Init();
            return result;
        }
    }
    public sealed class DiElement
    {
        public InjectionSource Source { get; init; }
        public IElementInjector Injector { get; init; }
        public override string ToString()
            => $"{Injector}:{Source}";
    }
    public readonly struct DiComponentContext
    {
        public WorldSetup World { get; init; }
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
        public IReadOnlyDictionary<Type, IDiComponent> WorldComponents { get; init; }
        public IReadOnlyDictionary<Type, IDiComponent> GameComponents { get; init; }
        public IReadOnlyCollection<Type> ForcedDynamicTypes { get; init; }
    }
    public readonly struct InitInjectorContext
    {
        public IReadOnlyDictionary<Type, IDiComponent> WorldComponents { get; init; }
        public IReadOnlyDictionary<Type, IDiComponent> GameComponents { get; init; }
        public IReadOnlyCollection<Type> ForcedDynamicTypes { get; init; }
    }
    public sealed class ConstructDiComponent : BaseDiComponent
    {
        public ConstructDiComponent(in DiComponentContext context) : base(context)
        {
        }

        public override object Create(IEntity entity)
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
    public interface IEntityFactory
    {
        IEntityInstaller Installer { get; }
        IEntity Create(in CreateEntityContext context);
    }
    public readonly struct CreateEntityContext
    {
        public string Name { get; init; }
    }
    public readonly struct PrepareEntityForSpawnContext
    {
        public IEntity Entity { get; init; }
        public IList<IDiComponent> ComponentsToBeInjected { get; init; }
        public bool DynamicOnly { get; init; }
    }
}