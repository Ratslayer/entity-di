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
			stack.Pop(value);
			stack.Push(value);
			stack.AutoFlushDisabled = false;
			stack.AutoFlushChangesIfDirty();
		}
		public static TValue RemoveAndPush<TValue>(
			this IStackValue<TValue> stack, TValue oldValue, TValue newValue)
		{
			stack.AutoFlushDisabled = true;
			stack.Pop(oldValue);
			stack.Push(newValue);
			stack.AutoFlushDisabled = false;
			stack.AutoFlushChangesIfDirty();
			return newValue;
		}
		public static void PushOrRemove<TValue>(this IStackValue<TValue> stack, TValue value, bool push)
		{
			if(push)
				stack.Push(value);
			else stack.Pop(value);
		}
	}
}