namespace EntityDi.Container;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class InjectAttribute : Attribute { }
public sealed class DiContainer : IResolver
{
	readonly Dictionary<Type, IIocElement> _elements = new();
	readonly DiContainer _parent;
	public string Name { get; init; }
	public bool Installed { get; private set; }
	public DiContainer(string name, DiContainer parent = null)
	{
		_parent = parent;
		Name = name;
	}
	public void Throw(string msg)
		=> throw new DiException($"[{Name}] {msg}");
	private void Bind(Type contract, IIocElement element)
	{
		if (Installed)
			Throw($"Attempting to bind {contract.Name} after installing.");
		element.Assert(contract, this);
		if (!_elements.TryAdd(contract, element))
			Throw($"Attempted dublicate binding for contact {contract.Name}");
	}
	public void BindLazy(Type contract, Type imp) => Bind(contract, new LazyIocElement(imp, false));
	public void Bind(Type contract, Type imp) => Bind(contract, new LazyIocElement(imp, true));
	public void BindInstance(Type contract, object instance) => Bind(contract, new InstanceIocElement(instance));
	public bool TryResolve(Type contract, out object result)
	{
		if (!Installed)
			Throw($"Attempting to resolve {contract.Name} before installing");
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