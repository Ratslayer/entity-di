namespace BB.Di
{
	public interface IEntityPool
	{
		void Return(IEntity entity);
		void Remove(IEntity entity);
	}
}
