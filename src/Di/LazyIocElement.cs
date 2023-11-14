using System;

namespace EntityDi.Container;
internal abstract record TypedIocStrategy(
	Type InstanceType,
	bool ResolveOnInstall,
	(Type, object)[] Args) : IIocStrategy
{
	object _instance;
	public void Assert(Type contract, DiContainer container)
	{
		if (!contract.IsAssignableFrom(InstanceType))
			throw new DiException($"[{container.Name}] {contract.Name} is not assignable from {InstanceType.Name}");
	}

	public object Resolve(DiContainer container)
	{
		_instance ??= container.Create(InstanceType, Args);
		return _instance;
	}
}
internal sealed record LazyIocElement(Type InstanceType, bool ResolveOnInstall, (Type, object)[] Args)
	: TypedIocStrategy(InstanceType, ResolveOnInstall, Args);
internal sealed record NonLazyIocElement(Type InstanceType, bool ResolveOnInstall, (Type, object)[] Args)
	: TypedIocStrategy(InstanceType, ResolveOnInstall, Args), IInstallElement;