using EntityDi.Container;
namespace EntityDi;
public interface IResolver
{
	void Bind(Type contract, Type imp);
	void BindLazy(Type contract, Type imp);
	void BindInstance(Type contract, object instance);
	bool TryResolve(Type contract, out object instance);
	void Inject(object instance);
	object Create(Type contract, IEnumerable<(Type, object)> args);
	void Install();
	//debug data
	string Name { get; }
	bool Installed { get; }
	void Throw(string message);
}
