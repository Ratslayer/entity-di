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
		: IAttachedSubscription
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
		public static void SubscribeExternal(this Entity entity, IEntitySubscription subscription)
		{
			if (entity)
				entity._ref.AddSubscription(subscription);
		}
		public static void UnsubscribeExternal(this Entity entity, IEntitySubscription subscription)
		{
			if (entity)
				entity._ref.RemoveSubscription(subscription);
		}
	}
}