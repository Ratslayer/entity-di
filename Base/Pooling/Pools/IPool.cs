public interface IPool
{
	public object GetFromPool();
	public void ReturnToPool(object obj);
}
