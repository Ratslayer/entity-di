using BB.Di;

namespace BB.Di
{
    public static class EntityUtils
    {
        public static void BindBaseEntityEvents(IDiContainer container)
        {
            container.Event<CreatedEvent>();
            container.Event<SpawnedEvent>();
            container.Event<PostSpawnedEvent>();
            container.Event<EnabledEvent>();
            container.Event<DisabledEvent>();
            container.Event<DespawnedEvent>();
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