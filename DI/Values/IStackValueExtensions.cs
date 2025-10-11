namespace BB.Di
{
	public static class IStackValueExtensions
	{
		public static StackValuePushDisposable<TValue> PushStackValue<TStack, TValue>(
			this Entity entity,
            in ValueWrapper<TValue> value)
			where TStack : IStackValue<TValue>
		{
			if (!entity.Has(out TStack stack))
				return default;

			return stack.Push(value);
		}
		public static void Repush<TValue>(this IStackValue<TValue> stack, in ValueWrapper<TValue> value)
		{
			stack.AutoFlushDisabled = true;
			stack.Pop(value);
			stack.Push(value);
			stack.AutoFlushDisabled = false;
			stack.AutoFlushChangesIfDirty();
		}
		public static TValue RemoveAndPush<TValue>(
			this IStackValue<TValue> stack, in ValueWrapper<TValue> oldValue, in ValueWrapper<TValue> newValue)
		{
			stack.AutoFlushDisabled = true;
			stack.Pop(oldValue);
			stack.Push(newValue);
			stack.AutoFlushDisabled = false;
			stack.AutoFlushChangesIfDirty();
			return newValue.Value;
		}
		public static void PushOrRemove<TValue>(this IStackValue<TValue> stack, in ValueWrapper<TValue> value, bool push)
		{
			if(push)
				stack.Push(value);
			else stack.Pop(value);
		}
	}
}