using EntityDi.Container;

namespace EntityDi;

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
		foreach (var info
			in ReflectionUtils.GetAllMethodsWithAttribute<SubscribeAttribute>(GetType()))
		{
			var args = info.GetParameters();
			if (args.Length != 1)
			{
				Logger.Log($"{selfType.Name}::{info.Name} has {args.Length} arguments. Can't subscribe it.", LogType.Error);
				continue;
			}
			var type = args[0].ParameterType;
			//create delegate
			var delegateType = typeof(Action<>).MakeGenericType(type);
			var action = info.CreateDelegate(delegateType, this);
			//create subscription
			var subscriptionType = typeof(EventSubscription<>).MakeGenericType(type);
			var constructor = subscriptionType.GetConstructors()[0];
			var constructorArgs = ReflectionUtils.GetArgArray(2);
			constructorArgs[0] = action;
			constructorArgs[1] = Entity;
			var subscription = constructor.Invoke(constructorArgs);
			Entity.AddSubscription((ISubscription)subscription);
		}
	}
}