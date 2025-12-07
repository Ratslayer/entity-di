using BB.Di;
namespace BB
{
	public abstract class EntitySystem : IEntityProvider
	{
		[Inject]
		readonly IEntity _entityRef;
		public Entity Entity => _entityRef.GetToken();
		public override string ToString()
			=> $"{GetType().Name} {Entity}";
	}
}