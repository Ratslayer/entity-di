using System;
using System.Collections.Generic;

namespace EntityDi.Container;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class InjectAttribute : Attribute { }
public sealed class DiContainer : IResolver
{
	readonly Dictionary<Type, IIocStrategy> _elements = new();
	readonly DiContainer _parent;
	public string Name { get; init; }
	public bool Installed { get; private set; }
	public DiContainer(string name, DiContainer parent = null)
	{
		_parent = parent;
		Name = name;
	}
	public DiException Exception(string msg)
		=> new DiException($"[{Name}] {msg}");
	private void Bind(Type contract, IIocStrategy element)
	{
		if (Installed)
			throw Exception($"Attempting to bind {contract.Name} after installing.");
		element.Assert(contract, this);
		if (!_elements.TryAdd(contract, element))
			throw Exception($"Attempted dublicate binding for contact {contract.Name}");
	}
	public void BindLazy(Type contract, Type imp, (Type, object)[] args)
		=> Bind(contract, new LazyIocElement(imp, false, args));
	public void Bind(Type contract, Type imp, (Type, object)[] args)
		=> Bind(contract, new NonLazyIocElement(imp, true, args));
	public void BindInstance(Type contract, object instance)
		=> Bind(contract, new InstanceIocElement(instance));
	public bool TryResolve(Type contract, out object result)
	{
		if (!Installed)
			throw Exception($"Attempting to resolve {contract.Name} before installing");
		var container = this;
		while (container is not null)
		{
			if (container._elements.TryGetValue(contract, out var element))
			{
				result = element.Resolve(this);
				return true;
			}
			container = container._parent;
		}
		result = default;
		return false;
	}
	public object Create(Type instanceType, IEnumerable<(Type, object)> instanceArgs)
	{
		var result = DiCreationUtils.Create(this, instanceType, instanceArgs);
		Inject(result);
		return result;
	}
	public void Inject(object instance)
	{
		DiInjectionUtils.Inject(this, instance);
	}
	public void Install()
	{
		Installed = true;
		foreach (var element in _elements.Values)
			if (element is IInstallElement)
				element.Resolve(this);
	}

}