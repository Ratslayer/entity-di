namespace BB
{
	public abstract class PooledObject<TSelf> : ProtectedPooledObject<TSelf>
		where TSelf : PooledObject<TSelf>, new()
	{
		public static TSelf GetPooled()
			=> GetPooledInternal();
	}
}