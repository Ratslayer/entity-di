using System;
namespace BB.Di
{
	public interface IDiContainer
	{
		void BindStrategy(Type contract, IDiStrategy strategy);
	}
	public interface
	public interface IEntityFactory
	{
		IEntity Create();
		void PrepareEntityForSpawn(in PrepareEntityForSpawnContext context);
	}
	public readonly struct PrepareEntityForSpawnContext
	{
		public IEntity Entity { get; init; }
		public IEntity Parent { get; init; }
	}
	public abstract class BaseEntityFactory : IEntityFactory
	{
		public IEntity Create()
		{
			throw new NotImplementedException();
		}

		public void PrepareEntityForSpawn(in PrepareEntityForSpawnContext context)
		{
			throw new NotImplementedException();
		}
	}
}