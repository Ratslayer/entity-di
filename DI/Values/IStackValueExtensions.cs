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

			return stack.Push(value);
		}
		public static void Repush<TValue>(this IStackValue<TValue> stack, TValue value)
		{
			stack.AutoFlushDisabled = true;
			stack.Remove(value);
			stack.Push(value);
			stack.AutoFlushDisabled = false;
			stack.AutoFlushChangesIfDirty();
		}
		public static void PushOrRemove<TValue>(this IStackValue<TValue> stack, TValue value, bool push)
		{
			if(push)
				stack.Push(value);
			else stack.Remove(value);
		}
	}
}