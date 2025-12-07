using BB.Di;

namespace BB.Di
{
    public static class EntityUtils
    {
        public static void BindBaseEntityEvents(IDiContainer container)
        {
            container.Event<EntityCreatedEvent>();
            container.Event<EntitySpawnedEvent>();
            container.Event<PostEntitySpawnedEvent>();
            container.Event<EntityEnabledEvent>();
            container.Event<EntityDisabledEvent>();
            container.Event<EntityDespawnedEvent>();
        }
    }
}
namespace BB
{
	public abstract class BaseInstaller : IEntityInstaller
	{
		public abstract string Name { get; }

		public void Install(IDiContainer container)
		{
            EntityUtils.BindBaseEntityEvents(container);
        }
	}
}