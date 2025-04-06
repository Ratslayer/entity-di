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
	public static class ExternalSubscriptionExtensions
	{
		public static void SubscribeExternal(this Entity entity, IEntityEventMethod subscription)
		{
			if (entity)
				entity._ref.AddSubscription(subscription);
		}
		public static void UnsubscribeExternal(this Entity entity, IEntityEventMethod subscription)
		{
			if (entity)
				entity._ref.RemoveSubscription(subscription);
		}
	}
}