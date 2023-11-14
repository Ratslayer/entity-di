using System;
using System.Collections.Generic;

namespace EntityDi;
public interface IResolver
{
	void Bind(Type contract, Type imp, (Type, object)[] args);
	void BindLazy(Type contract, Type imp, (Type, object)[] args);
	void BindInstance(Type contract, object instance);
	bool TryResolve(Type contract, out object instance);
	void Inject(object instance);
	object Create(Type contract, IEnumerable<(Type, object)> args);
	void Install();
	//debug data
	string Name { get; }
	bool Installed { get; }
	DiException Exception(string message);
}