using System;

namespace EntityDi.Container;

internal sealed record InstanceIocElement(object Instance) : IIocElement
{
	bool _injected;
	public void Assert(Type contract, DiContainer container)
	{
		if (Instance is null)
			throw container.Exception($"Attempted to assign null instance to {contract.Name}");
		var instanceType = Instance.GetType();
		if (!contract.IsAssignableFrom(instanceType))
			throw container.Exception($"Can't assign instance of type {instanceType.Name} to {contract.Name} contract");
	}

	public object Resolve(DiContainer container)
	{
		if (!_injected)
		{
			_injected = true;
			container.Inject(Instance);
		}
		return Instance;
	}
}
