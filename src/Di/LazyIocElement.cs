namespace EntityDi.Container;

internal sealed record LazyIocElement(Type InstanceType, bool ResolveOnInstall) : IIocElement, IInstallElement
{
	object _instance;
	public void Assert(Type contract, DiContainer container)
	{
		if (!contract.IsAssignableFrom(InstanceType))
			throw new DiException($"[{container.Name}] {contract.Name} is not assignable from {InstanceType.Name}");
	}

	public object Resolve(DiContainer container)
	{
		_instance ??= container.Create(InstanceType, Enumerable.Empty<(Type, object)>());
		return _instance;
	}
}