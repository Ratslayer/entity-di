using BB.Di;
using Cysharp.Threading.Tasks;
using System;
using System.Reflection;
using System.Threading;
namespace BB
{
	public static class DiEventsUtils
	{
		public static void LogError(object target, MethodInfo method, string msg)
			=> Log.Logger.Error($"{GetTypeMethodName(target, method)}: {msg}");
		public static string GetTypeMethodName(object target, MethodInfo method)
			=> $"{target.GetType().Name}.{method.Name}";
	
		enum MethodData
		{
			None,
			Required,
			Optional
		}
		static bool AssertIsValidMethod<AttributeType>(
			MethodInfo method,
			object target,
			MethodData data,
			bool allowAsync)
		{
			var p = method.GetParameters();
			if (method.ReturnType == typeof(void))
			{
				return Assert(data switch
				{
					MethodData.Required => p.Length == 1,
					MethodData.Optional => p.Length < 2,
					_ => p.Length == 0
				}, "Invalid number of method parameters");
			}
			else if (allowAsync && method.ReturnType == typeof(UniTaskVoid))
			{
				return p.Length switch
				{
					2 => Assert(data != MethodData.None, "Invalid number of arguments")
						&& Assert(p[1].ParameterType == typeof(CancellationToken),
							"Second arg can only be of type 'CancellationToken'"),
					1 => Assert(data != MethodData.None
							|| p[0].ParameterType == typeof(CancellationToken),
							"Invalid argument type."),
					0 => Assert(data != MethodData.Required, "Invalid number of arguments"),
					_ => Assert(false, "Invalid number of arguments"),
				};
			}
			else return Assert(false, "Only void and UniTaskVoid return types are allowed.");

			bool Assert(bool value, string errorMessage)
			{
				if (!value)
					LogError(target, method, $"<{typeof(AttributeType).Name}> {errorMessage}");
				return value;
			}

		}
		static object CreateEventSubscription(
			object target,
			MethodInfo method,
			Type genericSubscriptionType,
			Type wrappedType)
		{
			//get event type
			Type eventType;
			var args = method.GetParameters();
			if (wrappedType is null)
			{
				switch (args.Length)
				{
					case 1:
						eventType = args[0].ParameterType;
						break;
					case 2:
						eventType = args[1].ParameterType == typeof(CancellationToken)
							? args[0].ParameterType : args[1].ParameterType;
						break;
					default:
						LogError(target, method, 
							"Event binding attribute works only on methods with 1 arg + CancellationToken.");
						return null;
				}
			}
			else
			{
				if (args.Length != 0)
				{
					LogError(target, method,
						"Event binding attribute with wrapped type only works on methods with 0 args.");
					return null;
				}
				eventType = wrappedType;
			}
			var subscriptionType = genericSubscriptionType.MakeGenericType(eventType);
			//create subscription
			return Activator.CreateInstance(subscriptionType);
		}

		//static bool BindUpdateAction<AttributeType>(
		//	MethodInfo method,
		//	object target,
		//	IEntity entity,
		//	out Action<UpdateTime> action)
		//{
		//	if (!AssertIsValidMethod<AttributeType>(
		//		method,
		//		target,
		//		MethodData.Optional,
		//		false))
		//	{
		//		action = null;
		//		return false;
		//	}
		//	action = CreateAction<UpdateTime>(method, target, entity);
		//	return true;
		//}
		//static bool BindAction<AttributeType>(
		//	MethodInfo method, object target, IEntity entity, out Action action)
		//{
		//	if (!AssertIsValidMethod<OnEventAttachedAttribute>(
		//		method,
		//		target,
		//		MethodData.None,
		//		true))
		//	{
		//		action = null;
		//		return false;
		//	}
		//	action = CreateAction(method, target, entity);
		//	return action is not null;
		//}
		public static Action<T> CreateAction<T>(MethodInfo method, object target, IEntity entity)
		{
			var args = method.GetParameters();
			if (method.ReturnType == typeof(void))
			{
				switch (args.Length)
				{
					case 1:
						return (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), target, method);
					default:
						var action = (Action)Delegate.CreateDelegate(typeof(Action), target, method);
						return _ => action();
				};
			}
			else if (method.ReturnType == typeof(UniTaskVoid))
			{
				switch (args.Length)
				{
					case 2:
						var a2 = (Func<T, CancellationToken, UniTaskVoid>)Delegate
							.CreateDelegate(typeof(Func<T, CancellationToken, UniTaskVoid>), target, method);
						return t => a2(t, entity.Require<IEvent<EntityDespawnedEvent>>().NextEventCancellationToken).Forget();
					case 1:
						if (args[0].ParameterType == typeof(CancellationToken))
						{
							var a1 = (Func<CancellationToken, UniTaskVoid>)Delegate
								.CreateDelegate(typeof(Func<CancellationToken, UniTaskVoid>), target, method);
							return _ => a1(entity.Require<IEvent<EntityDespawnedEvent>>().NextEventCancellationToken).Forget();
						}
						else
						{
							var a1 = (Func<T, UniTaskVoid>)Delegate
								.CreateDelegate(typeof(Func<T, UniTaskVoid>), target, method);
							return t => a1(t).Forget();
						}
					default:
						var a0 = (Func<UniTaskVoid>)Delegate.CreateDelegate(typeof(Func<UniTaskVoid>), target, method);
						return _ => a0().Forget();
				}
			}
			else
			{
				LogError(target, method, "Action methods can only have return type of void or UniTaskVoid");
				return null;
			}
		}
		public static Action CreateAction(MethodInfo method, object target, IEntity entity)
		{
			if (method.ReturnType == typeof(void))
			{
				return (Action)Delegate.CreateDelegate(typeof(Action), target, method);
			}
			else if (method.ReturnType == typeof(UniTaskVoid))
			{
				if (method.GetParameters().Length == 0)
				{
					var asyncMethod
						= (Func<UniTaskVoid>)Delegate
						.CreateDelegate(typeof(Func<UniTaskVoid>), target, method);
					return () => asyncMethod().Forget();
				}
				else
				{
					var asyncMethod
						= (Func<CancellationToken, UniTaskVoid>)Delegate
						.CreateDelegate(typeof(Func<CancellationToken, UniTaskVoid>), target, method);
					return () => asyncMethod(entity.Require<IEvent<EntityDespawnedEvent>>().NextEventCancellationToken).Forget();
				}
			}
			else
			{
				LogError(target, method, "Action methods can only have return type of void or UniTaskVoid");
				return null;
			}
		}
	}
}