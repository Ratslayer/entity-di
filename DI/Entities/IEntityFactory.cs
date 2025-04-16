namespace BB
{
	public interface IEntityFactory
	{
		Entity SpawnEntity();
	}
	public interface IFactory<T>
	{
		T Create();
	}
}