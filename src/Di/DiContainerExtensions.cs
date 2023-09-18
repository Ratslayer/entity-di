using System;

namespace EntityDi.Container;

public static class DiContainerExtensions
{
	public static object Resolve(this DiContainer container, Type contract)
	{
		if (!container.TryResolve(contract, out var result))
			throw container.Exception($"Container or its parents do not contain a {contract.Name} bound contract.");
		return result;
	}
}
