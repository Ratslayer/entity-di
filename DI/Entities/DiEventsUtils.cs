using BB.Di;
using Cysharp.Threading.Tasks;
using System;
using System.Reflection;
using System.Threading;
namespace BB
{
	public static class DiEventsUtils
	{
		public static void BindMembersWithAttributes(IEntityEventsBinder binder, IEntity entity, object obj)
		{
			var type = obj.GetType();
			ReflectionUtils.ProcessAllMethods(type, Process);
			foreach (var info in ReflectionUtils.GetAllMembersWithAttribute<InjectOnAttachAttribute>(type))
				switch (info)
				{
					case PropertyInfo prop:
						binder.RegisterAttachedSubscription(new EntityPropertyAttachment
						{
							_info = prop,
							_target = obj
						});
						break;
					case FieldInfo field:
						binder.RegisterAttachedSubscription(new EntityFieldAttachment
						{
							_info = field,
							_target = obj
						});
						break;
				}
			void Process(MethodInfo[] methods)
			{
				Action action;
				foreach (var method in methods)
				{
					foreach (var attribute in method.GetCustomAttributes(false))
						switch (attribute)
						{
							case OnPostSpawnAttribute:
								if (BindAction<OnPostSpawnAttribute>(
									method, obj,entity, out action))
									binder.PostSpawnEvent += action;
								break;
							case OnUpdateAttribute:
								if (BindUpdateAction<OnUpdateAttribute>(
									method, obj, entity, out var updateAction))
									binder.UpdateEvent += updateAction;
								break;
							case OnFixedUpdateAttribute:
								if (BindUpdateAction<OnFixedUpdateAttribute>(
									method, obj, entity, out updateAction))
									binder.FixedUpdateEvent
										+= updateAction;
								break;
							case OnLateUpdateAttribute:
								if (BindUpdateAction<OnLateUpdateAttribute>(
									method, obj, entity, out updateAction))
									binder.LateUpdateEvent
										+= updateAction;
								break;
							case OnCreateAttribute:
								if (BindAction<OnCreateAttribute>(
									method, obj, entity, out action))
									binder.CreateEvent += action;
								break;
							case OnSpawnAttribute:
								if (BindAction<OnSpawnAttribute>(
									method, obj, entity, out action))
									binder.SpawnEvent += action;
								break;
							case OnDespawnAttribute:
								if (BindAction<OnDespawnAttribute>(
									method, obj, entity, out action))
									binder.DespawnEvent += action;
								break;
							case OnAttachAttribute:
								if (BindAction<OnAttachAttribute>(
									method, obj, entity, out action))
									binder.AttachEvent += action;
								break;
							case OnEnableAttribute:
								if (BindAction<OnEnableAttribute>(
									method, obj, entity, out action))
									binder.EnableEvent += action;
								break;
							case OnDisableAttribute:
								if (BindAction<OnDisableAttribute>(
									method, obj, entity, out action))
									binder.DisableEvent += action;
								break;
							case OnEventAttribute ea:
								if (ea._eventTypes is null || ea._eventTypes.Length == 0)
									BindEvent(binder, obj, method, entity, null);
								else foreach (var e in ea._eventTypes)
										BindEvent(binder, obj, method, entity, e);
								break;
							case OnEventAttachedAttribute eaa:
								if (eaa._eventTypes is null || eaa._eventTypes.Length == 0)
									BindEventAttached(binder, obj, method, entity, null);
								else foreach (var e in eaa._eventTypes)
										BindEventAttached(binder, obj, method, entity, e);
								break;
						}
				}
			}
		}

		public static void LogError(object target, MethodInfo method, string msg)
			=> Log.Logger.Error($"{GetTypeMethodName(target, method)}: {msg}");
		public static string GetTypeMethodName(object target, MethodInfo method)
			=> $"{target.GetType().Name}.{method.Name}";
		private static void BindEvent(
			IEntityEventsBinder binder,
			object target,
			MethodInfo method,
			IEntity entity,
			Type wrappedType)
		{
			if (!AssertIsValidMethod<OnEventAttribute>(
				method,
				target,
				MethodData.Optional,
				true))
				return;
		
			var subscription = (InternalSubscription)CreateEventSubscription(
				target,
				method,
				typeof(MethodInfoSubscription<>),
				wrappedType);
			subscription._method = method;
			subscription._target = target;
			subscription._entity = entity;
			subscription.Init();
			binder.RegisterSubscription(subscription);
		}
		static void BindEventAttached(
			IEntityEventsBinder binder,
			object target,
			MethodInfo method,
			IEntity entity,
			Type wrappedEvent)
		{
			var dataType = wrappedEvent is null ? MethodData.Required : MethodData.None;
			if (!AssertIsValidMethod<OnEventAttachedAttribute>(
				method,
				target,
				dataType,
				true))
				return;

			var subscription = (DiExternalSubscription)CreateEventSubscription(
				target,
				method,
				typeof(DiExternalSubscription<>),
				wrappedEvent);
			subscription._method = method;
			subscription._target = target;
			subscription.Init();
			binder.RegisterAttachedSubscription(subscription);
		}
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

		static bool BindUpdateAction<AttributeType>(
			MethodInfo method,
			object target,
			IEntity entity,
			out Action<UpdateTime> action)
		{
			if (!AssertIsValidMethod<AttributeType>(
				method,
				target,
				MethodData.Optional,
				false))
			{
				action = null;
				return false;
			}
			action = CreateAction<UpdateTime>(method, target, entity);
			return true;
		}
		static bool BindAction<AttributeType>(
			MethodInfo method, object target, IEntity entity, out Action action)
		{
			if (!AssertIsValidMethod<OnEventAttachedAttribute>(
				method,
				target,
				MethodData.None,
				true))
			{
				action = null;
				return false;
			}
			action = CreateAction(method, target, entity);
			return action is not null;
		}
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
						return t => a2(t, entity.DespawnCancellationToken).Forget();
					case 1:
						if (args[0].ParameterType == typeof(CancellationToken))
						{
							var a1 = (Func<CancellationToken, UniTaskVoid>)Delegate
								.CreateDelegate(typeof(Func<CancellationToken, UniTaskVoid>), target, method);
							return _ => a1(entity.DespawnCancellationToken).Forget();
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
					return () => asyncMethod(entity.DespawnCancellationToken).Forget();
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