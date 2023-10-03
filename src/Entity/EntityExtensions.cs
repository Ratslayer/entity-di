using System;
using System.Linq;

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
	public static bool TryResolve(this IEntity entity, Type type, out object instance)
		=> entity.Resolver.TryResolve(type, out instance);
	public static void Resolve<T>(this IEntity entity, ref T value)
		where T : class
	{
		if (value is null)
			value = entity.Resolve<T>();
	}
	public static T Resolve<T>(this IEntity entity)
	{
		if (!entity.TryResolve<T>(out var result))
			throw entity.Exception($"{typeof(T).Name} contract not found.");
		return result;
	}
	public static DiException Exception(this IEntity entity, string msg)
		=> entity.Resolver.Exception(msg);
	public static void Bind<ContractType, ImpType>(this IEntity entity)
		=> entity.Resolver.Bind(typeof(ContractType), typeof(ImpType));
	public static void Bind<T>(this IEntity entity)
		=> entity.Bind<T, T>();
	public static void BindLazy<ContractType, ImpType>(this IEntity entity)
		=> entity.Resolver.BindLazy(typeof(ContractType), typeof(ImpType));
	public static void BindLazy<T>(this IEntity entity)
		=> entity.BindLazy<T, T>();
	public static void Event<T>(this IEntity entity)
		=> entity.BindInstance<IPublisher<T>>(new Event<T>());
	public static void BindInstance<T>(this IEntity entity, T instance)
		=> entity.Resolver.BindInstance(typeof(T), instance);
	public static void AppendExplicit<T>(this IEntity entity, params (Type, object)[] args)
		=> entity.AppendExplicit(typeof(T), args);
	public static void Append<T>(this IEntity entity, params object[] args)
	{
		if(args is null)
		{
			entity.AppendExplicit<T>();
			return;
		}
		if (args.Contains(o => o is null))
			throw entity.Exception($"Tried to append a type with some args being null.");
		var convertedArgs = new (Type, object)[args.Length];//ArrayManager<(Type, object)>.GetArgArray(args.Length);
		for (int i = 0; i < convertedArgs.Length; i++)
			convertedArgs[i] = (args[i].GetType(), args[i]);
		AppendExplicit<T>(entity, convertedArgs);
	}
}