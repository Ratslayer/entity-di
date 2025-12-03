namespace BB.Di
{
	public interface IEntityPoolOld
	{
		void Return(IEntity entity);
		void Remove(IEntity entity);
	}
}
