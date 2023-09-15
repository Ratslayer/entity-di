namespace EntityDi;

public static class EntityExtensions
{
	public static bool TryResolve<T>(this IEntity entity, out T instance)
	{
		if (entity.Resolver.TryResolve(typeof(T), out var obj))
		{
			instance = (T)obj;
			return true;
		}
		instance = default;
		return false;
	}
	public static T Resolve<T>(this IEntity entity)
	{
		if (!entity.TryResolve<T>(out var result))
			entity.Throw($"{typeof(T).Name} contract not found.");
		return result;
	}
	public static void Throw(this IEntity entity, string msg)
		=> entity.Resolver.Throw(msg);
	public static void Bind<ContractType, ImpType>(this IEntity entity)
		=> entity.Resolver.Bind(typeof(ContractType), typeof(ImpType));
	public static void Bind<T>(this IEntity entity)
		=> entity.Bind<T, T>();
	public static void BindLazy<ContractType, ImpType>(this IEntity entity)
		=> entity.Resolver.BindLazy(typeof(ContractType), typeof(ImpType));
	public static void BindLazy<T>(this IEntity entity)
		=> entity.BindLazy<T, T>();
	public static void Event<T>(this IEntity entity)
		=> entity.Bind<Container.Event<T>>();
	public static void BindInstance<T>(this IEntity entity, T instance)
		=> entity.Resolver.BindInstance(typeof(T), instance);
}