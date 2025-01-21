using BB.Di;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Reflection;
namespace BB
{
	public static class DiEventsUtils
	{
		public static void BindMembersWithAttributes(IEntityEventsBinder binder, object obj)
		{
			var type = obj.GetType();
			ReflectionUtils.ProcessAllMethods(type, Process);
			foreach (var info in ReflectionUtils.GetAllMembersWithAttribute<InjectOnAttachAttribute>(type))
				switch (info)
				{
					case PropertyInfo prop:
						binder.RegisterExternalSubscription(new EntityPropertyAttachment
						{
							_info = prop,
							_target = obj
						});
						break;
					case FieldInfo field:
						binder.RegisterExternalSubscription(new EntityFieldAttachment
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
									method, obj, out action))
									binder.PostSpawnEvent += action;
								break;
							case OnUpdateAttribute:
								if (BindUpdateAction<OnUpdateAttribute>(
									method, obj, out var updateAction))
									binder.UpdateEvent += updateAction;
								break;
							case OnFixedUpdateAttribute:
								if (BindUpdateAction<OnFixedUpdateAttribute>(
									method, obj, out updateAction))
									binder.FixedUpdateEvent
										+= updateAction;
								break;
							case OnLateUpdateAttribute:
								if (BindUpdateAction<OnLateUpdateAttribute>(
									method, obj, out updateAction))
									binder.LateUpdateEvent
										+= updateAction;
								break;
							case OnCreateAttribute:
								if (BindAction<OnCreateAttribute>(
									method, obj, out action))
									binder.CreateEvent += action;
								break;
							case OnSpawnAttribute:
								if (BindAction<OnSpawnAttribute>(
									method, obj, out action))
									binder.SpawnEvent += action;
								break;
							case OnDespawnAttribute:
								if (BindAction<OnDespawnAttribute>(
									method, obj, out action))
									binder.DespawnEvent += action;
								break;
							case OnAttachAttribute:
								if (BindAction<OnAttachAttribute>(
									method, obj, out action))
									binder.AttachEvent += action;
								break;
							case OnEnableAttribute:
								if (BindAction<OnEnableAttribute>(
									method, obj, out action))
									binder.EnableEvent += action;
								break;
							case OnDisableAttribute:
								if (BindAction<OnDisableAttribute>(
									method, obj, out action))
									binder.DisableEvent += action;
								break;
							case OnEventAttribute ea:
								if (ea._eventTypes is null || ea._eventTypes.Length == 0)
									BindEvent(binder, obj, method, null);
								else foreach (var e in ea._eventTypes)
										BindEvent(binder, obj, method, e);
								break;
							case OnEventAttachedAttribute eaa:
								if (eaa._eventTypes is null || eaa._eventTypes.Length == 0)
									BindEventAttached(binder, obj, method, null);
								else foreach (var e in eaa._eventTypes)
										BindEventAttached(binder, obj, method, e);
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
			Type wrappedType)
		{
			if (!AssertEventMethodSignature<OnEventAttachedAttribute>(
				target, method, wrappedType))
				return;
			if (binder is not IEntity entity)
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
			Type wrappedEvent)
		{
			if (!AssertEventMethodSignature<OnEventAttachedAttribute>(
				target, method, wrappedEvent))
				return;
			var subscription = (DiExternalSubscription)CreateEventSubscription(
				target,
				method,
				typeof(DiExternalSubscription<>),
				wrappedEvent);
			subscription._method = method;
			subscription._target = target;
			subscription.Init();
			binder.RegisterExternalSubscription(subscription);
		}
		static bool AssertEventMethodSignature<AttributeType>(
			object target,
			MethodInfo method,
			Type wrappedType)
		{
			var numArgs = wrappedType is null ? 1 : 0;
			return AssertAttributeMethodSignature<OnEventAttachedAttribute>(
				target, method, numArgs);
		}
		static bool AssertAttributeMethodSignature<AttributeType>(
			object target, MethodInfo method, int numArgs)
		{
			if (method.GetParameters().Length != numArgs)
			{
				Log.Logger.Error(
					$"{typeof(AttributeType).Name} only works on methods with {numArgs} args.");
				return false;
			}
			return AssertReturnType<AttributeType>(method);
		}
		static bool AssertUpdateMethodSignature<AttributeType>(
			object target, MethodInfo method)
		{
			var args = method.GetParameters();
			if (args.Length != 1
				|| args[0].ParameterType != typeof(UpdateTime))
			{
				Log.Logger.Error(
					$"{typeof(AttributeType).Name} only works on methods " +
					$"with 1 {nameof(UpdateTime)} arg.");
				return false;
			}
			if (method.ReturnType != typeof(void))
			{
				Log.Logger.Error(
					$"{typeof(AttributeType).Name} only works on methods " +
					$"with void return type.");
				return false;
			}
			return true;
		}
		static bool AssertReturnType<AttributeType>(
			MethodInfo method)
		{
			if (method.ReturnType != typeof(void)
				&& method.ReturnType != typeof(IEnumerator)
				&& method.ReturnType != typeof(UniTaskVoid))
			{
				Log.Logger.Error(
					$"{typeof(AttributeType).Name} only works on methods with return types " +
					$"void, IEnumerator or async UniTaskVoid");
				return false;
			}
			return true;
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
				if (args.Length != 1)
				{
					LogError(target, method, "Event binding attribute only works on methods with 1 arg.");
					return null;
				}
				eventType = args[0].ParameterType;
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

		public static Action<T> CreateEventAction<T>(
			MethodInfo method,
			object target)
		{
			Action<T> action;
			var hasArgs = method.GetParameters().Length > 0;
			if (method.ReturnType == typeof(IEnumerator))
			{
				if (hasArgs)
				{
					var coroutine = CreateDelegate<Func<T, IEnumerator>>();
					action = arg => TimingUtils.StartCoroutine(coroutine(arg));
				}
				else
				{
					var coroutine = CreateDelegate<Func<IEnumerator>>();
					action = _ => TimingUtils.StartCoroutine(coroutine());
				}
			}
			else if (method.ReturnType == typeof(UniTaskVoid))
			{
				if (hasArgs)
				{
					var asyncMethod = CreateDelegate<Func<T, UniTaskVoid>>();
					action = arg => asyncMethod(arg).Forget();
				}
				else
				{
					var asyncMethod = CreateDelegate<Func<UniTaskVoid>>();
					action = _ => asyncMethod().Forget();
				}
			}
			else
			{
				if (hasArgs)
					action = CreateDelegate<Action<T>>();
				else
				{
					var a = CreateDelegate<Action>();
					action = _ => a();
				}
			}
			return action;
			TAction CreateDelegate<TAction>()
				where TAction : Delegate
				=> (TAction)Delegate.CreateDelegate(typeof(TAction), target, method);
		}
		static bool BindUpdateAction<AttributeType>(
			MethodInfo method,
			object target,
			out Action<UpdateTime> action)
		{
			if (!AssertUpdateMethodSignature<AttributeType>(
				target,
				method))
			{
				action = null;
				return false;
			}
			if (method.ReturnType == typeof(IEnumerator))
			{
				var coroutine = (Func<UpdateTime, IEnumerator>)
					Delegate.CreateDelegate(typeof(Func<IEnumerator>), target, method);
				action = time => TimingUtils.StartCoroutine(coroutine(time));
			}
			else if (method.ReturnType == typeof(UniTaskVoid))
			{
				var asyncMethod = (Func<UpdateTime, UniTaskVoid>)
					Delegate.CreateDelegate(typeof(Func<UniTaskVoid>), target, method);
				action = time => asyncMethod(time).Forget();
			}
			else action = (Action<UpdateTime>)
					Delegate.CreateDelegate(
						typeof(Action<UpdateTime>), target, method);
			return true;
		}
		static bool BindAction<AttributeType>(
			MethodInfo method, object target, out Action action)
		{
			if (!AssertAttributeMethodSignature<AttributeType>(
				target, method, 0))
			{
				action = null;
				return false;
			}
			if (method.ReturnType == typeof(IEnumerator))
			{
				var coroutine = (Func<IEnumerator>)
					Delegate.CreateDelegate(typeof(Func<IEnumerator>), target, method);
				action = () => TimingUtils.StartCoroutine(coroutine());
			}
			else if (method.ReturnType == typeof(UniTaskVoid))
			{
				var asyncMethod = (Func<UniTaskVoid>)
					Delegate.CreateDelegate(typeof(Func<UniTaskVoid>), target, method);
				action = () => asyncMethod().Forget();
			}
			else action = (Action)Delegate.CreateDelegate(typeof(Action), target, method);
			return true;
		}
	}
}