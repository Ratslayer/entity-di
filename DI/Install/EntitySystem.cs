using BB.Di;
namespace BB
{
	public abstract record EntitySystem : IEntityProvider
	{
		[Inject]
		readonly IEntity _entityRef;
		public Entity Entity => _entityRef.GetToken();

		public override string ToString()
			=> $"{GetType().Name} {Entity}";

		protected void Despawn() => Entity.Despawn();
	}
}