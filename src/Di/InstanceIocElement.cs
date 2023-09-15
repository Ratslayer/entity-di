namespace EntityDi.Container;

internal sealed record InstanceIocElement(object Instance) : IIocElement
{
	public void Assert(Type contract, DiContainer container)
	{
		if (Instance is null)
			container.Throw($"Attempted to assign null instance to {contract.Name}");
		var instanceType = Instance.GetType();
		if (!contract.IsAssignableFrom(instanceType))
			container.Throw($"Can't assign instance of type {instanceType.Name} to {contract.Name} contract");
	}

	public object Resolve(DiContainer container)
	{
		return Instance;
	}
}
