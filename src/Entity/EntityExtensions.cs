using System;
using System.Linq;

namespace EntityDi;

public static class EntityExtensions
{
	public static bool TryResolve<T>(this IEntity entity, out T instance)
	{
		if (entity is not null
			&& entity.Resolver.TryResolve(typeof(T), out var obj))
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
			throw entity.Exception($"{typeof(T).FullName} contract not found.");
		return result;
	}
	public static DiException Exception(this IEntity entity, string msg)
		=> entity.Resolver.Exception(msg);
	public static void Bind<ContractType, ImpType>(this IEntity entity, params object[] args)
		=> entity.Resolver.Bind(typeof(ContractType), typeof(ImpType), ConvertArgs<ImpType>(entity, args));
	public static void BindExplicit<ContractType, ImpType>(this IEntity entity, params (Type, object)[] args)
		=> entity.Resolver.Bind(typeof(ContractType), typeof(ImpType), args);
	public static void Bind<T>(this IEntity entity, params object[] args)
		=> entity.Bind<T, T>(args);
	public static void BindLazy<ContractType, ImpType>(this IEntity entity, params object[] args)
		=> entity.Resolver.BindLazy(typeof(ContractType), typeof(ImpType), ConvertArgs<ImpType>(entity, args));
	public static void BindLazy<T>(this IEntity entity, params object[] args)
		=> entity.BindLazy<T, T>(args);
	public static void Event<T>(this IEntity entity)
		=> entity.BindInstance<IPublisher<T>>(new Event<T>());
	public static void BindInstance<T>(this IEntity entity, T instance)
		=> entity.Resolver.BindInstance(typeof(T), instance);
	public static void AppendExplicit<T>(this IEntity entity, params (Type, object)[] args)
		=> entity.AppendExplicit(typeof(T), args);
	public static void Append<T>(this IEntity entity, params object[] args)
	{
		var convertedArgs = ConvertArgs<T>(entity, args);
		AppendExplicit<T>(entity, convertedArgs);
	}
	static (Type, object)[] ConvertArgs<T>(IEntity entity, object[] args)
	{
		if (args is null || args.Length == 0)
			return Array.Empty<(Type, object)>();
		if (args.Contains(o => o is null))
			throw entity.Exception($"Tried to create {typeof(T).FullName} with some args being null.");
		var convertedArgs = new (Type, object)[args.Length];
		for (int i = 0; i < convertedArgs.Length; i++)
			convertedArgs[i] = (args[i].GetType(), args[i]);
		return convertedArgs;
	}
	public static bool Has<VarType>(this IEntity entity, out VarType var)
		where VarType : IVariable
	{
		if (entity is null)
		{
			var = default;
			return false;
		}
		return entity.TryResolve(out var);
	}
	public static bool Has<T1, T2>(this IEntity entity, out T1 var1, out T2 var2)
		where T1 : IVariable
		where T2 : IVariable

	{
		if (entity is null)
		{
			var1 = default;
			var2 = default;
			return false;
		}
		var result = entity.Has(out var1);
		result &= entity.Has(out var2);
		return result;
	}
	public static bool Has<T1, T2, T3>(this IEntity entity, out T1 var1, out T2 var2, out T3 var3)
		where T1 : IVariable
		where T2 : IVariable
		where T3 : IVariable

	{
		if (entity is null)
		{
			var1 = default;
			var2 = default;
			var3 = default;
			return false;
		}
		var result = entity.Has(out var1);
		result &= entity.Has(out var2);
		result &= entity.Has(out var3);
		return result;
	}
}