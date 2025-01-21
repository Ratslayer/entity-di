using System;
using BB.Di;
namespace BB
{
	public readonly partial struct Entity
	{
		public Entity AttachedToEntity
			=> this && _ref is not null
			? (_ref as EntityImpl).AttachedToEntity.GetToken()
			: default;
	}
	public sealed record OnDespawnExternalSubscription(Action action)
		: IExternalSubscription
	{
		public void Subscribe(IEntity entity)
		{
			(entity as EntityImpl).DespawnEvent += action;
		}

		public void Unsubscribe(IEntity entity)
		{
			(entity as EntityImpl).DespawnEvent -= action;
		}
	}
	public static class ExternalSubscriptionExtensions
	{
		public static void AddToEntity(this IExternalSubscription subscription, Entity entity)
		{
			if (entity)
				(entity._ref as EntityImpl).AddExternalSubscription(subscription);
		}
		public static void SubscribeExternal<TEvent>(this Entity entity, Action<TEvent> action)
		{
			if (!entity)
				return;
			var subscription = new OnEventExternalSubscription<TEvent>(action);
			subscription.AddToEntity(entity);
		}
	}
}