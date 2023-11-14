using EntityDi.Container;
using System;
using System.Reflection;

namespace EntityDi;
public static class SubscriptionUtils
{
	public static void CreateSubscriptions<AttributeType>(object target, ILogger logger, Action<ISubscription> subscriptionProcessor)
	{
		var selfType = target.GetType();
		foreach (var info
			in ReflectionUtils.GetAllMethodsWithAttribute<SubscribeAttribute>(selfType))
		{
			var args = info.GetParameters();
			if (args.Length != 1)
			{
				logger.Log($"{selfType.Name}::{info.Name} has {args.Length} arguments. Can't subscribe it.", LogType.Error);
				continue;
			}
			var type = args[0].ParameterType;
			//create delegate
			var delegateType = typeof(Action<>).MakeGenericType(type);
			var action = info.CreateDelegate(delegateType, target);
			//create subscription
			var subscriptionType = typeof(EventSubscription<>).MakeGenericType(type);
			var constructor = subscriptionType.GetConstructors()[0];
			var constructorArgs = ArrayManager<object>.GetArgArray(2);
			constructorArgs[0] = action;
			var subscription = constructor.Invoke(constructorArgs);
			subscriptionProcessor((ISubscription)subscription);
		}
	}
}
public abstract record EntitySystem
{
	[Inject]
	void Init(IEntity entity, ILogger logger)
	{
		Entity = entity;
		Logger = logger;
		CreateSubscriptions();
	}

	public IEntity Entity { get; private set; }
	public ILogger Logger { get; private set; }
	void CreateSubscriptions()
	{
		var selfType = GetType();
		foreach (var method in ReflectionUtils.GetAllMethodsWithAttribute<SubscribeAttribute>(selfType))
			if (CreateSubscription<ISubscription>(method, typeof(EventSubscription<>), out var subscription))
				Entity.AddSubscription(subscription);
		foreach (var method in ReflectionUtils.GetAllMethodsWithAttribute<SubscribeAttachmentAttribute>(selfType))
			if (CreateSubscription<IAttachedSubscription>(method, typeof(AttachedEventSubscription<>), out var subscription))
				Entity.AddAttachedSubscription(subscription);
		bool CreateSubscription<SubscriptionType>(MethodInfo info, Type subscriptionType, out SubscriptionType subscription)
		{
			var args = info.GetParameters();
			if (args.Length != 1)
			{
				Logger.Log($"{selfType.Name}::{info.Name} has {args.Length} arguments. Can't subscribe it.", LogType.Error);
				subscription = default;
			}
			var type = args[0].ParameterType;
			//create delegate
			var delegateType = typeof(Action<>).MakeGenericType(type);
			var action = info.CreateDelegate(delegateType, this);
			//create subscription
			var subscriptionGenericType = subscriptionType.MakeGenericType(type);
			var constructor = subscriptionGenericType.GetConstructors()[0];
			var constructorArgs = ArrayManager<object>.GetArgArray(1);
			constructorArgs[0] = action;
			subscription = (SubscriptionType)constructor.Invoke(constructorArgs);
			return true;
		}
	}
}