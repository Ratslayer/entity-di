namespace BB.Di
{
	public sealed class PooledValue<T>
		: ProtectedPooledObject<PooledValue<T>>,
		IReadOnlyValue<T>
	{
		public T Value { get; private set; }

		public static PooledValue<T> GetPooled(T value)
		{
			var result = GetPooledInternal();
			result.Value = value;
			return result;
		}
	}
}