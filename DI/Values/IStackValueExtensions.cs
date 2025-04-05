namespace BB.Di
{
	public static class IStackValueExtensions
	{
		public static StackValuePushDisposable<TValue> PushStackValue<TStack, TValue>(
			this Entity entity,
			TValue value)
			where TStack : IStackValue<TValue>
		{
			if (!entity.Has(out TStack stack))
				return default;

			var pooledWrapper = PooledValue<TValue>.GetPooled(value);
			return stack.Push(pooledWrapper);
		}
	}
}